using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Balık botu ilk başlatıldığında SADECE 1 KERE çalışan başlangıç hazırlık fonksiyonudur.
    /// Modüler yapıda bağımsız bir fonksiyon olarak tasarlanmıştır.
    /// 
    /// AKIŞ SIRASI:
    /// 1. Seçili olan oyun penceresini ekranda en öne getirir.
    /// 2. Pencerenin tam ortasına 1 kere sağ tıklar (Right Click).
    /// 3. 'F' tuşuna kesintisiz 3 saniye (3000 ms) basılı tutup bırakır.
    /// 4. 'G' tuşuna kesintisiz 3 saniye (3000 ms) basılı tutup bırakır.
    /// </summary>
    public static class FishBotStartupFunction
    {
        /// <summary>
        /// Başlangıç sekansını verilen istemci için asenkron olarak çalıştırır.
        /// </summary>
        /// <param name="clientInfo">İşlem yapılacak aktif istemci bilgisi</param>
        /// <param name="cancellationToken">İptal isteği bayrağı</param>
        public static async Task ExecuteAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || cancellationToken.IsCancellationRequested) return;

            // HWND Geçerlilik Kontrolü
            if (clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle))
            {
                Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) için geçerli pencere bulunamadı.");
                return;
            }

            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç sekansı başlatılıyor...");

            // 1. Bot çalışır çalışmaz seçili olan oyun penceresi en öne getirilecek
            GameWindowProcessHelper.BringWindowToFront(clientInfo.Handle);
            await Task.Delay(400, cancellationToken);

            // 2. F ve G tuşlarına basmadan önce pencerenin ortasına 1 kere sağ tıklayıp bıraksın
            PerformWindowCenterRightClick(clientInfo.Handle);
            await Task.Delay(300, cancellationToken);

            // 3. Sırayla önce F tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'F' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_F, 3000, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'F' tuşu bırakıldı.");

            await Task.Delay(300, cancellationToken);

            // 4. Sonra G tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'G' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_G, 3000, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'G' tuşu bırakıldı.");

            await Task.Delay(300, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç sekansı başarıyla tamamlandı.");
        }

        /// <summary>
        /// Belirtilen sanal tuşa (VK) verilen süre boyunca basılı tutar.
        /// Oyun motorlarının (DirectX / DirectInput) tuş basılı tutma durumunu kesintisiz algılaması için
        /// periyodik donanımsal tekrar (typematic repeat) darbeleri üretir ve süre bitiminde tuşu serbest bırakır.
        /// </summary>
        /// <param name="vk">Sanal tuş kodu (Virtual Key Code)</param>
        /// <param name="durationMs">Basılı tutulacak milisaniye süresi (Örn: 3000 ms)</param>
        /// <param name="cancellationToken">İptal jetonu</param>
        public static async Task HoldKeyAsync(uint vk, int durationMs, CancellationToken cancellationToken)
        {
            byte scanCode = (byte)Win32Native.MapVirtualKey(vk, 0);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // İlk tuş basma (Key Down)
                Win32Native.keybd_event((byte)vk, scanCode, 0, 0);

                while (stopwatch.ElapsedMilliseconds < durationMs)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    // DirectInput / Oyun döngülerinin tuşun basılı kaldığını sürekli hissetmesi için repeat sinyali
                    Win32Native.keybd_event((byte)vk, scanCode, 0, 0);
                    await Task.Delay(40, cancellationToken);
                }
            }
            finally
            {
                // Tuşu garanti olarak serbest bırak (Key Up)
                Win32Native.keybd_event((byte)vk, scanCode, Win32Native.KEYEVENTF_KEYUP, 0);
            }
        }

        /// <summary>
        /// Verilen pencerenin (HWND) iç alanının tam merkez koordinatını hesaplayıp fareyi oraya konumlandırır ve sağ tıklar.
        /// </summary>
        public static void PerformWindowCenterRightClick(IntPtr hWnd)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd)) return;

                int targetScreenX = 0;
                int targetScreenY = 0;

                if (Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect) && clientRect.Width > 0 && clientRect.Height > 0)
                {
                    Win32Native.POINT centerPt = new Win32Native.POINT(clientRect.Width / 2, clientRect.Height / 2);
                    if (Win32Native.ClientToScreen(hWnd, ref centerPt))
                    {
                        targetScreenX = centerPt.X;
                        targetScreenY = centerPt.Y;
                    }
                }

                if (targetScreenX == 0 && targetScreenY == 0)
                {
                    if (Win32Native.GetWindowRect(hWnd, out Win32Native.RECT winRect))
                    {
                        targetScreenX = winRect.Left + (winRect.Width / 2);
                        targetScreenY = winRect.Top + (winRect.Height / 2);
                    }
                }

                if (targetScreenX > 0 && targetScreenY > 0)
                {
                    Debug.WriteLine($"[FishBotStartupFunction] Pencere merkezine fare taşınıyor ve sağ tıklanıyor: ({targetScreenX}, {targetScreenY})");
                    Win32Native.SetCursorPos(targetScreenX, targetScreenY);
                    Thread.Sleep(50);
                    Win32Native.mouse_event(Win32Native.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    Thread.Sleep(50);
                    Win32Native.mouse_event(Win32Native.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FishBotStartupFunction] Sağ tık hatası: {ex.Message}");
            }
        }
    }
}
