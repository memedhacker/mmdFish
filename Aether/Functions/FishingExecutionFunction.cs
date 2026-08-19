using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using Aether.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aether.Functions
{
    /// <summary>
    /// Balık botunun asıl balık tutma döngüsünü (Yem seçme, olta atma, chat alanı taraması,
    /// balık filtre kontrolü, AutoPass yönetimi, menü başlığı tespiti, eşzamanlı waypoint takibi,
    /// animasyon iptali ve kesintisiz döngüye devam etme) yürüten merkezi modüler fonksiyon sınıfı.
    /// </summary>
    public static class FishingExecutionFunction
    {
        #region Şablon Adı -> Filtre Tablosu Eşleme Sözlüğü

        /// <summary>
        /// TemplateConstants.FishNames içindeki dosya adlarını FishFilter tablosundaki Türkçe öğe anahtarlarına birebir eşler.
        /// </summary>
        private static readonly Dictionary<string, string> FishTemplateToFilterKey = new(StringComparer.OrdinalIgnoreCase)
        {
            // Rare (Nadir Balıklar)
            ["altin_sudak"] = "Altın_Sudak_Balığı",
            ["aynali_sazan"] = "Aynalı_Sazan",
            ["kadife"] = "Kadife_Balığı",
            ["kral_yengeci"] = "Kral_Yengeci",
            ["kurbaga_baligi"] = "Kurbağa_Balığı",
            ["palamut"] = "Palamut_Balığı",
            ["sevimli_balik"] = "Sevimli_Balık",
            ["yabbie"] = "Yabbie_Yengeci",
            ["yilan_basi_baligi"] = "Yılan_Başı_Balığı",

            // Common (Yaygın Balıklar)
            ["buyuk_sudak"] = "Büyük_Sudak_Balığı",
            ["dere_alabaligi"] = "Dere_Alabalığı",
            ["gokkusagi_alabaligi"] = "Gökkuşağı_Alabalığı",
            ["hamsi"] = "Hamsi",
            ["levrek"] = "Levrek",
            ["lufer"] = "Lüfer_Balığı",
            ["nehir_alabaligi"] = "Nehir_Alabalığı",
            ["ot_sazani"] = "Ot_Sazanı",
            ["ringa"] = "Ringa_Balığı",
            ["sazan"] = "Sazan",
            ["som"] = "Som_Balığı",
            ["sudak"] = "Sudak_Balığı",
            ["tekir"] = "Tekir_Balığı",
            ["yayin_baligi"] = "Yayın_Balığı",
            ["zargana"] = "Zargana",

            // Others (Diğer Öğeler)
            ["altin_anahtar"] = "Altın_Anahtar",
            ["altin_parcasi"] = "Altın_Parçası",
            ["altin_yuzuk"] = "Altın_Yüzük",
            ["beyaz_sac_boyasi"] = "Beyaz_Saç_Boyası",
            ["bilge_kralin_eldiveni"] = "Bilge_Kralın_Eldiveni",
            ["bilge_kralin_sembolu"] = "Bilge_Kralın_Sembolü",
            ["deniz_kizi_anahtari"] = "Deniz_Kızı_Anahtarı",
            ["gorunmezlik_pelerini"] = "Görünmezlik_Pelerini",
            ["gumus_anahtar"] = "Gümüş_Anahtar",
            ["kahverengi_sac_boyasi"] = "Kahverengi_Saç_Boyası",
            ["kirmizi_sac_boyasi"] = "Kırmızı_Saç_Boyası",
            ["lucy_yuzugu"] = "Lucy'nin_Yüzüğü",
            ["sac_boyasi_temizleyici"] = "Saç_Boyası_Temizleyici",
            ["sari_sac_boyasi"] = "Sarı_Saç_Boyası",
            ["siyah_sac_boyasi"] = "Siyah_Saç_Boyası"
        };

        #endregion

        /// <summary>
        /// <summary>
        /// Tek bir balık tutma döngüsünü 6 adımda kesintisiz yürütür:
        /// 1. Envanter ve Slot Kontrolü (Boş slot yoksa Öldürme ve Pişirme)
        /// 2. Yem Kontrolü ve Hazırlık (Yem yoksa BuyWorm/Balıkçı kontrolü, Yem varsa sağ tık & oltalama hızı beklemesi)
        /// 3. Oltayı Fırlatma ve İlk Kontroller (Space ile atış & ChatBox Tutamazsın kontrolü)
        /// 4. Filtreleme ve Karar (Balığı Tut / AutoPass kontrolü, kapalıysa çıkış butonu & animasyon iptali)
        /// 5. Balık Tutma (Mini-Oyun & Eşzamanlı Waypoint takibi & Animasyon iptali)
        /// 6. Sonuç ve Döngü (Yakalandıysa 100ms bekle, kaçtıysa/diğer doğrudan 1. adıma dön)
        /// </summary>
        public static async Task ExecuteFishingCycleAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle) || cancellationToken.IsCancellationRequested)
                return;

            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);

            // =========================================================================
            // 1. ADIM: Envanter ve Slot Kontrolü
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Adım 1] Envanterdeki boş slot sayısı taranıyor...");
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            int emptyCount = ScanEmptySlots(clientInfo.Handle);
            BotLogger.LogInfo(clientInfo.Id, $"[Adım 1] Envanter (InventoryFishArea) boş slot sayısı: {emptyCount}");

            // Eğer boş slot yoksa (EmptySlot == 0): Öldürme, yere atma ve pişirme süreçlerini sırayla çalıştır
            if (emptyCount == 0)
            {
                BotLogger.LogWarning(clientInfo.Id, "🛑 [Adım 1] Çanta tamamen dolu (InventoryFishArea boş slot: 0)! Önce balık öldürme başlatılıyor...");

                // 1. Öldürme sürecini çalıştır
                await FishKillingFunction.ExecuteKillingProcessAsync(clientInfo, settings, cancellationToken);

                // 2. Yere atma sürecini çalıştır
                BotLogger.LogInfo(clientInfo.Id, "🗑️ [Adım 1] Öldürme tamamlandı, yere atma sürecine geçiliyor...");
                await FishDropFunction.ExecuteDropProcessAsync(clientInfo, settings, cancellationToken);

                // 3. Pişirme sürecini çalıştır
                BotLogger.LogInfo(clientInfo.Id, "🔥 [Adım 1] Yere atma tamamlandı, balık pişirme sürecine geçiliyor...");
                bool cookedSuccess = await FishCookingFunction.ExecuteCookingProcessAsync(clientInfo, settings, cancellationToken);

                // Pişirme fonksiyonu kendi içinde D4 (boş slot kontrolü) yapar ve açılamazsa botu durdurur
                if (!cookedSuccess)
                {
                    return;
                }

                BotLogger.LogSuccess(clientInfo.Id, "✅ [Adım 1] Boş slot açıldı. 2. adıma geçiliyor.");
            }

            // =========================================================================
            // 2. ADIM: Yem Kontrolü ve Hazırlık
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Adım 2] Envanterdeki yemler taranıyor...");
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            List<TemplateMatchResult> baitMatches = ScanBaits(clientInfo.Handle);

            // Eğer yem yoksa:
            if (baitMatches.Count == 0)
            {
                if (settings.BuyWormEnabled)
                {
                    // yem satın al == true: balıkçı bulma ve yem alma işlemleri
                    BotLogger.LogWarning(clientInfo.Id, "⚠️ [Adım 2] Envanterde yem kalmadı! BuyWorm ayarı aktif, balıkçıdan yem satın alma başlatılıyor...");
                    await StartupFishermanFunction.ExecuteAsync(clientInfo, cancellationToken);
                    await Task.Delay(300, cancellationToken);

                    // Satın alma sonrası tekrar yemleri tara
                    baitMatches = ScanBaits(clientInfo.Handle);
                }
                else
                {
                    // yem satın al == false: Botu durdur / Uyarı ver
                    BotLogger.LogWarning(clientInfo.Id, "🛑 [Adım 2] Envanterde hiç yem kalmadı ve 'BuyWorm' pasif! Bot durduruluyor.");
                    FishBotService.Instance.StopFishBot(clientInfo.Id);
                    BringMainFormToFront();
                    return;
                }

                if (baitMatches.Count == 0)
                {
                    BotLogger.LogError(clientInfo.Id, "❌ [Adım 2] Balıkçı işlemlerine rağmen envanterde yem bulunamadı! Bot durduruluyor.");
                    FishBotService.Instance.StopFishBot(clientInfo.Id);
                    BringMainFormToFront();
                    return;
                }
            }

            BotLogger.LogInfo(clientInfo.Id, $"[Adım 2] Envanterde toplam {baitMatches.Count} adet yem slotu tespit edildi.");

            // Yem varsa: Rastgele bir yeme sağ tıkla
            int randomIndex = Random.Shared.Next(baitMatches.Count);
            var chosenBait = baitMatches[randomIndex];

            int targetLocalX = RegionConstants.InventoryPosition.StartX + chosenBait.Location.X + (chosenBait.Bounds.Width / 2);
            int targetLocalY = RegionConstants.InventoryPosition.StartY + chosenBait.Location.Y + (chosenBait.Bounds.Height / 2);

            BotLogger.LogInfo(clientInfo.Id, $"Rastgele yem seçildi (#{randomIndex + 1}, Konum: {targetLocalX}, {targetLocalY}). Sağ tıklanıyor...");

            await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, targetLocalX, targetLocalY, fastMove: false, cancellationToken: cancellationToken);
            await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // Oltalama hızı beklemesi yap (Min-Max ms)
            int minSpeed = Math.Max(30, settings.FishingSpeedMinMs);
            int maxSpeed = Math.Max(minSpeed, settings.FishingSpeedMaxMs);
            int castDelayMs = Random.Shared.Next(minSpeed, maxSpeed + 1);

            BotLogger.LogInfo(clientInfo.Id, $"Yem takıldı. Oltalama hızı gecikmesi ({castDelayMs}ms) bekleniyor...");
            await Task.Delay(castDelayMs, cancellationToken);

            // =========================================================================
            // 3. ADIM: Oltayı Fırlatma ve İlk Kontroller
            // =========================================================================
            // Space tuşuna basarak olta at
            BotLogger.LogInfo(clientInfo.Id, "[Adım 3] Space tuşuna basılarak olta atıldı (Balık tutma başlatıldı).");
            await StartupCameraFunction.HoldKeyAsync(Win32Native.VK_SPACE, 80, cancellationToken);
            await Task.Delay(Random.Shared.Next(250, 400), cancellationToken);

            // ChatBox'ı tara (Balık Adı / AutoPass / Tutamazsın)
            BotLogger.LogInfo(clientInfo.Id, "[Adım 3] ChatBox alanı taranıyor (Balık adları, AutoPass ve Tutamazsın bekleniyor)...");

            var candidateTemplates = new List<string>();
            candidateTemplates.AddRange(TemplateConstants.FishNames.All);
            candidateTemplates.AddRange(TemplateConstants.AutoPass.All);
            candidateTemplates.Add(TemplateConstants.Waypoints.Tutamazsin);

            TemplateMatchResult? matchedResult = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                // ChatArea taraması yapmadan önce HotSaleBox kontrolü
                await CloseHotSalePopupIfExistsAsync(clientInfo, cancellationToken);

                using (Bitmap? chatBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.ChatBoxPosition))
                {
                    if (chatBmp != null)
                    {
                        var leftmostMatch = TemplateConstants.FindLeftmostMatch(chatBmp, candidateTemplates, minThreshold: 0.75);
                        if (leftmostMatch != null && leftmostMatch.IsSuccess)
                        {
                            matchedResult = leftmostMatch;
                            BotLogger.LogSuccess(clientInfo.Id, $"[CHAT TESPİTİ] Şablon bulundu: '{leftmostMatch.TemplateName}' | Benzerlik: %{leftmostMatch.Confidence * 100:F1} | Konum: ({leftmostMatch.Location.X}, {leftmostMatch.Location.Y})");
                            break;
                        }
                    }
                }

                await Task.Delay(80, cancellationToken);
            }

            if (matchedResult == null || cancellationToken.IsCancellationRequested)
                return;

            // Eğer "Tutamazsın" mesajı geldiyse: Botu durdur ve alan uyarısı göster
            if (matchedResult.TemplatePath == TemplateConstants.Waypoints.Tutamazsin || matchedResult.TemplateName.Equals("tutamazsin", StringComparison.OrdinalIgnoreCase))
            {
                BotLogger.LogError(clientInfo.Id, $"🚫 'Tutamazsin' waypoint'i tespit edildi! Client #{clientInfo.Id} balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                BringMainFormToFront();
                ShowTutamazsinWarning(clientInfo.Id);
                return;
            }

            // =========================================================================
            // 4. ADIM: Filtreleme ve Karar
            // =========================================================================
            bool isAutoPass = TemplateConstants.AutoPass.All.Contains(matchedResult.TemplatePath);
            bool isCatchEnabled = !isAutoPass && IsCatchEnabled(settings, matchedResult.TemplateName);

            if (isAutoPass)
            {
                BotLogger.LogInfo(clientInfo.Id, $"🛡️ AutoPass şablonu ('{matchedResult.TemplateName}') tespit edildi. Balık oyunu pas geçilecek, çıkış butonuna tıklanacak.");
            }
            else if (!isCatchEnabled)
            {
                BotLogger.LogWarning(clientInfo.Id, $"⚠️ '{matchedResult.TemplateName}' için 'Balığı Tut' / 'Yakala' seçeneği kapalı. Balık oyunu pas geçilecek, çıkış butonuna tıklanacak.");
            }
            else
            {
                BotLogger.LogSuccess(clientInfo.Id, $"✅ '{matchedResult.TemplateName}' için 'Balığı Tut' / 'Yakala' seçeneği aktif. Balık oyunu başlatılacak.");
            }

            // FishingMenuTitle başlığını bekle (Timeout süresi ile: 15 sn)
            BotLogger.LogInfo(clientInfo.Id, "Fisherman alanında 'FishingMenuTitle' başlığı aranıyor (Timeout: 15 sn)...");
            bool menuTitleFound = false;
            var titleWaitStopwatch = Stopwatch.StartNew();

            while (!cancellationToken.IsCancellationRequested && titleWaitStopwatch.ElapsedMilliseconds < 15000)
            {
                using (Bitmap? searchBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.FisherManSearchArea))
                {
                    if (searchBmp != null)
                    {
                        var menuMatch = TemplateConstants.Match(searchBmp, TemplateConstants.WindowParts.FishingMenuTitle, threshold: 0.80);
                        if (menuMatch.IsSuccess)
                        {
                            menuTitleFound = true;
                            BotLogger.LogSuccess(clientInfo.Id, $"[BALIK OYUNU] FishingMenuTitle tespit edildi! Konum: ({menuMatch.Location.X}, {menuMatch.Location.Y}), Benzerlik: %{menuMatch.Confidence * 100:F1}");
                            break;
                        }
                    }
                }

                await Task.Delay(80, cancellationToken);
            }

            if (!menuTitleFound || cancellationToken.IsCancellationRequested)
            {
                BotLogger.LogWarning(clientInfo.Id, "⚠️ FishingMenuTitle zaman aşımına uğradı veya bulunamadı. 1. Adıma dönülüyor...");
                return;
            }

            // Eğer Hayır / AutoPass ise:
            if (isAutoPass || !isCatchEnabled)
            {
                // FishingMenuExitButton'a tıkla
                int exitLocalX = RegionConstants.FishingMenuExitButtonPosition.StartX + (RegionConstants.FishingMenuExitButtonPosition.Width / 2);
                int exitLocalY = RegionConstants.FishingMenuExitButtonPosition.StartY + (RegionConstants.FishingMenuExitButtonPosition.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"FishingMenuExitButton ({exitLocalX}, {exitLocalY}) tıklanarak menü kapatılıyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, exitLocalX, exitLocalY, fastMove: false, cancellationToken: cancellationToken);
                await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "FishingMenuExitButton tıklandı ve menü kapatıldı.");

                // Animasyon iptali yap
                await PerformAnimationCancelAsync(clientInfo, settings, cancellationToken);

                // 1. Adıma dön
                BotLogger.LogInfo(clientInfo.Id, "Pas geçme tamamlandı. 1. Adıma dönülüyor...");
                return;
            }

            // =========================================================================
            // 5. ADIM: Balık Tutma (Mini-Oyun)
            // =========================================================================
            // Eğer Evet ise: Eşzamanlı olarak Mini-Oyun ve Chat Waypoint takibini yürüt
            using var phaseCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            TemplateMatchResult? matchedWaypoint = null;

            try
            {
                var minigameTask = FishingMinigameFunction.ExecuteMinigameAsync(clientInfo, phaseCts.Token);
                var waypointWatcherTask = Task.Run(async () =>
                {
                    matchedWaypoint = await WatchWaypointsAsync(clientInfo, phaseCts);
                }, phaseCts.Token);

                // Her iki görev eşzamanlı çalışır, waypoint eşleştiğinde phaseCts iptal edilerek sonlandırılır
                await Task.WhenAll(minigameTask, waypointWatcherTask);
            }
            catch (OperationCanceledException)
            {
                // Temiz iptal
            }

            // Animasyon iptali yap
            await PerformAnimationCancelAsync(clientInfo, settings, cancellationToken);

            // =========================================================================
            // 6. ADIM: Sonuç ve Döngü
            // =========================================================================
            // Waypoint sonucunu kontrol et:
            if (matchedWaypoint != null && (matchedWaypoint.TemplatePath == TemplateConstants.Waypoints.Tutamazsin || matchedWaypoint.TemplateName.Equals("tutamazsin", StringComparison.OrdinalIgnoreCase)))
            {
                BotLogger.LogError(clientInfo.Id, $"🚫 'Tutamazsin' waypoint'i tespit edildi! Client #{clientInfo.Id} balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                BringMainFormToFront();
                ShowTutamazsinWarning(clientInfo.Id);
                return;
            }

            if (matchedWaypoint != null && (
                matchedWaypoint.TemplatePath == TemplateConstants.Waypoints.YakalananBalik || 
                matchedWaypoint.TemplateName.Equals("yakalanan_balik", StringComparison.OrdinalIgnoreCase) ||
                matchedWaypoint.TemplatePath == TemplateConstants.Waypoints.Yapboz ||
                matchedWaypoint.TemplateName.Equals("yapboz", StringComparison.OrdinalIgnoreCase)))
            {
                // Balık veya yapboz yakalandı: 100 ms bekle ve 1. Adıma dön
                BotLogger.LogSuccess(clientInfo.Id, $"🎣 '{matchedWaypoint.TemplateName}' tespit edildi! 100ms bekleniyor ve 1. Adıma dönülüyor...");
                await Task.Delay(100, cancellationToken);
                return;
            }
            else
            {
                // Balık kaçtı veya diğer durumlar: 1. Adıma dön
                BotLogger.LogInfo(clientInfo.Id, "Balık kaçtı veya tur tamamlandı. 1. Adıma dönülüyor...");
                return;
            }
        }

        /// <summary>
        /// Envanterdeki tüm yem (yem.png ve yem200.png) slotlarını tarar ve çakışmaları ayıklayarak döndürür.
        /// </summary>
        private static List<TemplateMatchResult> ScanBaits(IntPtr hWnd)
        {
            var baitMatches = new List<TemplateMatchResult>();

            using (Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryPosition))
            {
                if (invBmp == null) return baitMatches;

                var yemMatches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60);
                var yem200Matches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Yem200, threshold: 0.60);

                baitMatches.AddRange(yemMatches);

                foreach (var m200 in yem200Matches)
                {
                    if (!baitMatches.Any(b => Math.Abs(b.Location.X - m200.Location.X) < 14 && Math.Abs(b.Location.Y - m200.Location.Y) < 14))
                    {
                        baitMatches.Add(m200);
                    }
                }
            }

            return baitMatches;
        }

        #region Animasyon İptali (Animation Cancel)

        /// <summary>
        /// Seçilen moda göre ("mount" / "armor") karakterin balık tutma animasyonunu iptal eder.
        /// </summary>
        public static async Task PerformAnimationCancelAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return;

            if (settings.AnimationMode == "armor")
            {
                // Zırh Değiştir Modu: Envanterin sol üst köşesinden 20px sağa, 20px aşağıya (İlk Slot) sağ tıkla
                int firstSlotLocalX = RegionConstants.InventoryPosition.StartX + 20;
                int firstSlotLocalY = RegionConstants.InventoryPosition.StartY + 20;

                BotLogger.LogInfo(clientInfo.Id, $"Animasyon iptali yapılıyor (Mod: Zırh Değiştir - İlk Slot: {firstSlotLocalX}, {firstSlotLocalY})...");

                await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, firstSlotLocalX, firstSlotLocalY, fastMove: false, cancellationToken: cancellationToken);
                await Task.Delay(Random.Shared.Next(100, 180), cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "Animasyon iptali (Zırh) tamamlandı.");
            }
            else
            {
                // Binek Kullan Modu (Varsayılan): Oltalama hızı aralığında rastgele gecikmeyle 2x Ctrl + G
                int minSpeed = Math.Max(30, settings.FishingSpeedMinMs);
                int maxSpeed = Math.Max(minSpeed, settings.FishingSpeedMaxMs);
                int randomSpeedDelay = Random.Shared.Next(minSpeed, maxSpeed + 1);

                BotLogger.LogInfo(clientInfo.Id, $"Animasyon iptali yapılıyor (Mod: Binek Kullan - 2x Ctrl+G, Gecikme: {randomSpeedDelay}ms)...");

                await SendDoubleCtrlGAsync(randomSpeedDelay, cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "Animasyon iptali (Binek - Ctrl+G) tamamlandı.");
            }
        }

        /// <summary>
        /// Donanımsal 2 kez ardışık Ctrl + G tuş kombinasyonu basar.
        /// </summary>
        private static async Task SendDoubleCtrlGAsync(int intervalDelayMs, CancellationToken cancellationToken)
        {
            byte ctrlScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_CONTROL, 0);
            byte gScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_G, 0);

            // --- 1. KEZ CTRL + G ---
            Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, 0, 0);
            Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, 0, 0);
            await Task.Delay(35, cancellationToken);
            Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, Win32Native.KEYEVENTF_KEYUP, 0);
            Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, Win32Native.KEYEVENTF_KEYUP, 0);

            // Oltalama hızı aralığında hesaplanan dinamik bekleme süresi
            await Task.Delay(intervalDelayMs, cancellationToken);

            // --- 2. KEZ CTRL + G ---
            Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, 0, 0);
            Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, 0, 0);
            await Task.Delay(35, cancellationToken);
            Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, Win32Native.KEYEVENTF_KEYUP, 0);
            Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, Win32Native.KEYEVENTF_KEYUP, 0);
        }

        #endregion

        /// <summary>
        /// İstemcinin kayıtlı ayarlarında (FishFilter) belirtilen balık/öğe için "Balığı Tut" veya "Yakala" seçeneğinin aktif olup olmadığını kontrol eder.
        /// </summary>
        public static bool IsCatchEnabled(FishBotSettings settings, string templateName)
        {
            if (settings == null || settings.FishFilter == null) return true;

            // 1. Doğrudan sözlükten eşleştirme
            if (FishTemplateToFilterKey.TryGetValue(templateName, out string? filterKey) && !string.IsNullOrEmpty(filterKey))
            {
                foreach (var category in settings.FishFilter.Values)
                {
                    if (category.TryGetValue(filterKey, out var filterItem))
                    {
                        return filterItem.GetCheck("Balığı Tut", false) || filterItem.GetCheck("Yakala", false);
                    }
                }
            }

            // 2. Normalizasyon ile akıllı eşleştirme (Fallback)
            string normTemplate = NormalizeKey(templateName);

            foreach (var category in settings.FishFilter.Values)
            {
                foreach (var kvp in category)
                {
                    string itemKey = kvp.Key;
                    var filterItem = kvp.Value;
                    string normItem = NormalizeKey(itemKey);

                    bool matches = normItem == normTemplate ||
                                   normItem.StartsWith(normTemplate) ||
                                   normTemplate.StartsWith(normItem) ||
                                   (normItem.Contains(normTemplate) && normTemplate.Length >= 4);

                    if (matches)
                    {
                        return filterItem.GetCheck("Balığı Tut", false) || filterItem.GetCheck("Yakala", false);
                    }
                }
            }

            return true; // Eşleşmeyen veya tabloda olmayan öğeler varsayılan olarak yakalanır
        }

        /// <summary>
        /// Türkçe karakterleri, alt çizgileri ve boşlukları temizleyerek anahtar normalizasyonu yapar.
        /// </summary>
        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;
            return key.ToLowerInvariant()
                      .Replace("ı", "i")
                      .Replace("ğ", "g")
                      .Replace("ü", "u")
                      .Replace("ş", "s")
                      .Replace("ö", "o")
                      .Replace("ç", "c")
                      .Replace("_", "")
                      .Replace(" ", "")
                      .Replace("'", "")
                      .Replace("-", "");
        }

        /// <summary>
        /// ChatArea bölgesini tarayarak TemplateConstants.Waypoints.All ve TemplateConstants.AutoPass.All şablonlarını arar.
        /// Bir waypoint veya AutoPass eşleştiğinde log atar, eşleşen sonucu döner ve phaseCts'i iptal ederek balık oyununu sonlandırır.
        /// </summary>
        private static async Task<TemplateMatchResult?> WatchWaypointsAsync(ClientInfo clientInfo, CancellationTokenSource phaseCts)
        {
            BotLogger.LogInfo(clientInfo.Id, "ChatArea üzerinde Waypoint ve AutoPass şablonları taranıyor...");

            var watchTemplates = new List<string>();
            watchTemplates.AddRange(TemplateConstants.Waypoints.All);
            watchTemplates.AddRange(TemplateConstants.AutoPass.All);

            try
            {
                while (!phaseCts.Token.IsCancellationRequested)
                {
                    // ChatArea taraması yapmadan önce HotSaleBox kontrolü
                    await CloseHotSalePopupIfExistsAsync(clientInfo, phaseCts.Token);

                    using (Bitmap? chatBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.ChatBoxPosition))
                    {
                        if (chatBmp != null)
                        {
                            var leftmostMatch = TemplateConstants.FindLeftmostMatch(chatBmp, watchTemplates, minThreshold: 0.75);
                            if (leftmostMatch != null && leftmostMatch.IsSuccess)
                            {
                                BotLogger.LogSuccess(clientInfo.Id, $"[CHAT / WAYPOINT TESPİTİ] '{leftmostMatch.TemplateName}' şablonu eşleşti! Benzerlik: %{leftmostMatch.Confidence * 100:F1}, Konum: ({leftmostMatch.Location.X}, {leftmostMatch.Location.Y})");
                                BotLogger.LogInfo(clientInfo.Id, "Waypoint / AutoPass tespit edildiği için balık oyunu tamamlandı.");

                                // Her iki eşzamanlı görevi iptal et ve çık
                                phaseCts.Cancel();
                                return leftmostMatch;
                            }
                        }
                    }

                    await Task.Delay(60, phaseCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Beklenen iptal
            }

            return null;
        }

        /// <summary>
        /// ChatArea taraması yapılmadan önce HotSaleBox alanında SaleExitButton şablonunu arar.
        /// Eğer bulunursa pencereyi kapatmak için butonun üzerine hızlıca tıklar.
        /// </summary>
        public static async Task CloseHotSalePopupIfExistsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return;

            try
            {
                using (Bitmap? hotSaleBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.HotSaleBox))
                {
                    if (hotSaleBmp != null)
                    {
                        var match = TemplateConstants.Match(hotSaleBmp, TemplateConstants.WindowParts.SaleExitButton, threshold: 0.70);
                        if (match.IsSuccess)
                        {
                            int targetLocalX = RegionConstants.HotSaleBox.StartX + match.Location.X + (match.Bounds.Width / 2);
                            int targetLocalY = RegionConstants.HotSaleBox.StartY + match.Location.Y + (match.Bounds.Height / 2);

                            BotLogger.LogInfo(clientInfo.Id, $"🔥 HotSaleBox içinde 'SaleExitButton' tespit edildi ({targetLocalX}, {targetLocalY}). Kapatmak için hızlıca tıklanıyor...");

                            await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, targetLocalX, targetLocalY, fastMove: true, cancellationToken: cancellationToken);
                            await Task.Delay(Random.Shared.Next(80, 150), cancellationToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogWarning(clientInfo.Id, $"HotSaleBox kontrolünde hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Ana formu (MainForm) simge durumundan çıkarıp ekranın en önüne getirir.
        /// </summary>
        public static void BringMainFormToFront()
        {
            try
            {
                if (Application.OpenForms.Count > 0)
                {
                    var mainForm = Application.OpenForms[0];
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        Action restoreAction = () =>
                        {
                            if (mainForm.WindowState == FormWindowState.Minimized)
                            {
                                mainForm.WindowState = FormWindowState.Normal;
                            }
                            mainForm.Show();
                            mainForm.Activate();
                            Win32Native.SetForegroundWindow(mainForm.Handle);
                        };

                        if (mainForm.InvokeRequired)
                        {
                            mainForm.BeginInvoke(restoreAction);
                        }
                        else
                        {
                            restoreAction();
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// 'Tutamazsin' uyarısını ekranda MessageBox olarak asenkron/ana iş parçacığında gösterir.
        /// </summary>
        public static void ShowTutamazsinWarning(int clientId)
        {
            try
            {
                string message = $"Client #{clientId} balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruldu.";
                string title = "Balık Tutma Alanı Uyarısı";

                if (Application.OpenForms.Count > 0)
                {
                    var mainForm = Application.OpenForms[0];
                    if (mainForm != null && !mainForm.IsDisposed)
                    {
                        mainForm.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(mainForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                        return;
                    }
                }

                Task.Run(() =>
                {
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
            }
            catch { }
        }

        /// <summary>
        /// InventoryFishArea içerisindeki 5x7 (35 slot) ızgarada fareyi sütun sütun yukarı-aşağı gezdirir.
        /// </summary>
        public static async Task HoverAcrossInventoryFishAreaAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            const int cols = 5;
            const int rows = 7;

            double slotW = (double)RegionConstants.InventoryFishArea.Width / cols;
            double slotH = (double)RegionConstants.InventoryFishArea.Height / rows;

            for (int col = 0; col < cols; col++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Çift sütunlarda yukarıdan aşağıya (0 -> 6), tek sütunlarda aşağıdan yukarıya (6 -> 0)
                bool goDown = (col % 2 == 0);
                int startRow = goDown ? 0 : rows - 1;
                int endRow = goDown ? rows : -1;
                int step = goDown ? 1 : -1;

                for (int row = startRow; goDown ? row < endRow : row > endRow; row += step)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    int targetX = RegionConstants.InventoryFishArea.StartX + (int)((col + 0.5) * slotW) + Random.Shared.Next(-2, 3);
                    int targetY = RegionConstants.InventoryFishArea.StartY + (int)((row + 0.5) * slotH) + Random.Shared.Next(-2, 3);

                    await HumanMouseService.Instance.MoveMouseToLocalAsync(hWnd, targetX, targetY, cancellationToken);
                    await Task.Delay(Random.Shared.Next(40, 70), cancellationToken);
                }
            }

            // Gezme bittiğinde fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(hWnd, cancellationToken);
        }

        /// <summary>
        /// InventoryFishArea bölgesindeki boş slot (EmptySlot) sayısını Non-Maximum Suppression ile sayar.
        /// </summary>
        public static int ScanEmptySlots(IntPtr hWnd)
        {
            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return 0;

                var emptySlots = TemplateConstants.MatchAll(fishAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);
                emptySlots.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

                var uniqueEmpty = new List<TemplateMatchResult>();
                foreach (var slot in emptySlots)
                {
                    if (!uniqueEmpty.Any(existing => Math.Abs(existing.Location.X - slot.Location.X) < 16 && Math.Abs(existing.Location.Y - slot.Location.Y) < 16))
                    {
                        uniqueEmpty.Add(slot);
                    }
                }
                return uniqueEmpty.Count;
            }
        }
    }
}
