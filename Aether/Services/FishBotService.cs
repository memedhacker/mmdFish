using Aether.Models;
using Aether.Native;
using Aether.States;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Services
{
    /// <summary>
    /// Balık botunun arka planda kesintisiz (sürekli) çalışmasını yöneten singleton servis.
    /// Her bir client için bağımsız çalışma döngüleri (Task), iptal mekanizmaları (CancellationTokenSource) ve süre takibi yürütür.
    /// </summary>
    public class FishBotService
    {
        private static readonly Lazy<FishBotService> _instance = new Lazy<FishBotService>(() => new FishBotService());
        public static FishBotService Instance => _instance.Value;

        // Her bir ClientId için çalışan CancellationTokenSource kaynakları
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _runningTasks = new ConcurrentDictionary<int, CancellationTokenSource>();

        // Her bir ClientId için bot çalışma başlangıç zamanları
        private readonly ConcurrentDictionary<int, DateTime> _startTimes = new ConcurrentDictionary<int, DateTime>();

        /// <summary> Bir client için balık botu başladığında veya durduğunda tetiklenen olay (ClientId, IsRunning). </summary>
        public event EventHandler<(int ClientId, bool IsRunning)>? OnFishBotStateChanged;

        /// <summary>
        /// Belirtilen client için balık botunun çalışıp çalışmadığını kontrol eder.
        /// </summary>
        public bool IsFishBotRunning(int clientId)
        {
            return _runningTasks.ContainsKey(clientId);
        }

        /// <summary>
        /// Belirtilen client için geçen çalışma süresini döndürür.
        /// </summary>
        public TimeSpan GetBotElapsedTime(int clientId)
        {
            if (_startTimes.TryGetValue(clientId, out var startTime))
            {
                return DateTime.Now - startTime;
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Belirtilen client için bot durumunu tersine çevirir (çalışıyorsa durdurur, duruyorsa başlatır).
        /// HWND seçilip seçilmediğini kontrol eder ve sonucu mesaj ile döndürür.
        /// </summary>
        public (bool Success, string Message) ToggleFishBot(ClientInfo clientInfo)
        {
            if (clientInfo == null) return (false, "Geçersiz istemci nesnesi.");

            if (IsFishBotRunning(clientInfo.Id))
            {
                StopFishBot(clientInfo.Id);
                return (true, "Balık botu durduruldu.");
            }
            else
            {
                return StartFishBot(clientInfo);
            }
        }

        /// <summary>
        /// Belirtilen client için sürekli çalışan balık botu döngüsünü başlatır.
        /// HWND seçilip seçilmediğini kontrol eder.
        /// </summary>
        public (bool Success, string Message) StartFishBot(ClientInfo clientInfo)
        {
            if (clientInfo == null) return (false, "Geçersiz istemci nesnesi.");

            if (IsFishBotRunning(clientInfo.Id))
            {
                return (true, "Bot zaten çalışıyor.");
            }

            // HWND Kontrolü: İstemci için geçerli bir oyun penceresi seçilmiş mi?
            if (clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle))
            {
                return (false, $"'{clientInfo.Name}' için henüz geçerli bir oyun penceresi (HWND) seçilmemiş veya bağlı olan pencere kapanmış.\n\nLütfen önce Balık Botu sayfasından bir oyun penceresi seçip 'Seçili Client'a HWND Bağla' butonuna basın.");
            }

            var cts = new CancellationTokenSource();
            if (_runningTasks.TryAdd(clientInfo.Id, cts))
            {
                _startTimes[clientInfo.Id] = DateTime.Now;

                // Aktif HWND oyun penceresini en öne getir
                if (clientInfo.Handle != IntPtr.Zero && Win32Native.IsWindow(clientInfo.Handle))
                {
                    Helpers.GameWindowProcessHelper.BringWindowToFront(clientInfo.Handle);
                }

                // State katmanına balık botunun çalıştığını duyur
                ClientState.Instance.IsFishBotRunning = true;
                OnFishBotStateChanged?.Invoke(this, (clientInfo.Id, true));

                // Kesintisiz döngü görevini arka planda başlat
                Task.Run(() => FishBotLoopAsync(clientInfo, cts.Token), cts.Token);
                return (true, "Balık botu başarıyla başlatıldı.");
            }

            return (false, "Bot görevi başlatılamadı.");
        }

        /// <summary>
        /// Belirtilen client için çalışan balık botu döngüsünü güvenli şekilde durdurur.
        /// </summary>
        public void StopFishBot(int clientId)
        {
            _startTimes.TryRemove(clientId, out _);

            if (_runningTasks.TryRemove(clientId, out var cts))
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch
                {
                    // İptal esnasındaki istisnaları yut
                }

                // Eğer çalışan başka bot kalmadıysa global state'i güncelle
                if (_runningTasks.IsEmpty)
                {
                    ClientState.Instance.IsFishBotRunning = false;
                }

                OnFishBotStateChanged?.Invoke(this, (clientId, false));
            }
        }

        /// <summary>
        /// Çalışan tüm istemcilerdeki (Client) balık botlarını tek seferde durdurur (Acil Stop).
        /// </summary>
        public void StopAllBots()
        {
            foreach (var clientId in _runningTasks.Keys)
            {
                StopFishBot(clientId);
            }
        }

        /// <summary>
        /// HWND bağlı (geçerli oyun penceresine sahip) olan tüm istemcileri tek seferde başlatır.
        /// Başarıyla başlatılan veya halihazırda çalışan istemci sayısını döndürür.
        /// </summary>
        public int StartAllBotsWithHwnd()
        {
            int startedCount = 0;
            foreach (var client in ClientState.Instance.AllClients)
            {
                if (client.Handle != IntPtr.Zero && Win32Native.IsWindow(client.Handle))
                {
                    var (success, _) = StartFishBot(client);
                    if (success)
                    {
                        startedCount++;
                    }
                }
            }
            return startedCount;
        }

        /// <summary>
        /// Arka planda sürekli çalışan ana balık botu döngüsü.
        /// İptal bayrağı (cancellationToken) tetiklenene kadar dairesel olarak çalışır.
        /// </summary>
        private async Task FishBotLoopAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Bot başlar başlamaz seçili pencereyi en öne getir ve başlangıç sekansını (G 5sn basılı tut + F basıp bırak) 1 kez çalıştır
                await Helpers.FishBotEngineHelper.ExecuteInitialSequenceAsync(clientInfo, cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Modüler Helper sınıfından tekil balık tutma adımını çalıştır
                    await Helpers.FishBotEngineHelper.ExecuteSingleCycleAsync(clientInfo, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Döngü durdurulduğunda fırlatılan normal iptal istisnası
            }
            catch (Exception ex)
            {
                // Beklenmeyen döngü hatalarında güvenli çıkış
                System.Diagnostics.Debug.WriteLine($"FishBotLoop Hata (Client #{clientInfo.Id}): {ex.Message}");
            }
            finally
            {
                _startTimes.TryRemove(clientInfo.Id, out _);
                if (_runningTasks.TryRemove(clientInfo.Id, out var _))
                {
                    if (_runningTasks.IsEmpty)
                    {
                        ClientState.Instance.IsFishBotRunning = false;
                    }
                    OnFishBotStateChanged?.Invoke(this, (clientInfo.Id, false));
                }
            }
        }
    }
}
