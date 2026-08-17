using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using Aether.States;
using System;
using System.Collections.Generic;
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
        /// Tek bir balık tutma döngüsünü baştan sona çalıştırır:
        /// 1. Envanter bölgesindeki yemleri tarar, sayar ve listeler (Yem bittiyse ve BuyWorm açıksa marketten yem alır).
        /// 2. Bulunan yemler arasından rastgele bir tanesini seçip insansı fare hareketiyle sağ tıklar.
        /// 3. Space tuşuna basarak oltayı atar.
        /// 4. ChatBoxPosition alanını tarayarak balık adları ve AutoPass şablonlarını arar.
        /// 5. Balık veya AutoPass bulunduğunda filtre kontrolü yapar ("Balığı Tut" / "Yakala").
        /// 6. Eğer "Balığı Tut" kapalıysa veya AutoPass ise: FishingMenuTitle beklenir, FishingMenuExitButton'a tıklanır ve Animasyon İptali yapılır.
        /// 7. Eğer "Balığı Tut" açıksa: FishingMenuTitle bulunduğunda eşzamanlı olarak FishingMinigameFunction ve Waypoint takibi başlatılır.
        /// 8. ChatArea'da Waypoint şablonu eşleştiğinde fonksiyonlar durdurulur ve Animasyon İptali yapılarak döngü başarıyla tamamlanır.
        /// </summary>
        public static async Task ExecuteFishingCycleAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle) || cancellationToken.IsCancellationRequested)
                return;

            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);

            // =========================================================================
            // 1. ADIM: Envanter bölgesindeki yemleri tara ve listele
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "Envanterdeki yemler taranıyor...");
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            List<TemplateMatchResult> baitMatches = ScanBaits(clientInfo.Handle);

            // Yem kontrolü (Yem bittiyse ve BuyWorm açıksa balıkçıdan yem satın al)
            if (baitMatches.Count == 0)
            {
                if (settings.BuyWormEnabled)
                {
                    BotLogger.LogWarning(clientInfo.Id, "Envanterde yem kalmadı! BuyWorm ayarı aktif, balıkçıdan yem satın alma başlatılıyor...");
                    await StartupFishermanFunction.ExecuteAsync(clientInfo, cancellationToken);
                    await Task.Delay(300, cancellationToken);

                    // Satın alma sonrası tekrar yemleri tara
                    baitMatches = ScanBaits(clientInfo.Handle);
                }

                if (baitMatches.Count == 0)
                {
                    BotLogger.LogWarning(clientInfo.Id, "Envanterde hiç yem (solucan/paket) bulunamadı! Bot durduruluyor.");
                    FishBotService.Instance.StopFishBot(clientInfo.Id);
                    return;
                }
            }

            BotLogger.LogInfo(clientInfo.Id, $"Envanterde toplam {baitMatches.Count} adet yem slotu tespit edildi.");

            // =========================================================================
            // 2. ADIM: Bulunan yemler arasında rastgele bir tanesini seç ve sağ tıkla
            // =========================================================================
            int randomIndex = Random.Shared.Next(baitMatches.Count);
            var chosenBait = baitMatches[randomIndex];

            int targetLocalX = RegionConstants.InventoryPosition.StartX + chosenBait.Location.X + (chosenBait.Bounds.Width / 2);
            int targetLocalY = RegionConstants.InventoryPosition.StartY + chosenBait.Location.Y + (chosenBait.Bounds.Height / 2);

            BotLogger.LogInfo(clientInfo.Id, $"Rastgele yem seçildi (#{randomIndex + 1}, Konum: {targetLocalX}, {targetLocalY}). Sağ tıklanıyor...");

            await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, targetLocalX, targetLocalY, fastMove: false, cancellationToken: cancellationToken);
            await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // Oltalama hızı (FishingSpeedMinMs - FishingSpeedMaxMs) aralığında dinamik rastgele bekleme
            int minSpeed = Math.Max(30, settings.FishingSpeedMinMs);
            int maxSpeed = Math.Max(minSpeed, settings.FishingSpeedMaxMs);
            int castDelayMs = Random.Shared.Next(minSpeed, maxSpeed + 1);

            BotLogger.LogInfo(clientInfo.Id, $"Yem takıldı. Oltalama hızı gecikmesi ({castDelayMs}ms) bekleniyor...");
            await Task.Delay(castDelayMs, cancellationToken);

            // =========================================================================
            // 3. ADIM: Space tuşuna basarak oltayı at
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "Space tuşuna basılarak olta atıldı (Balık tutma başlatıldı).");
            await StartupCameraFunction.HoldKeyAsync(Win32Native.VK_SPACE, 80, cancellationToken);
            await Task.Delay(Random.Shared.Next(250, 400), cancellationToken);

            // =========================================================================
            // 4. ADIM: ChatBox alanını oku ve balık adları / AutoPass şablonlarını bekle
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "ChatBox alanı taranıyor (Balık adları ve AutoPass şablonları bekleniyor)...");

            var candidateTemplates = new List<string>();
            candidateTemplates.AddRange(TemplateConstants.FishNames.All);
            candidateTemplates.AddRange(TemplateConstants.AutoPass.All);
            candidateTemplates.Add(TemplateConstants.Waypoints.Tutamazsin);

            TemplateMatchResult? matchedResult = null;

            while (!cancellationToken.IsCancellationRequested)
            {
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

            // Tutamazsin Kontrolü (Chat'te burada balık tutamazsın uyarısı geldiyse)
            if (matchedResult.TemplatePath == TemplateConstants.Waypoints.Tutamazsin || matchedResult.TemplateName.Equals("tutamazsin", StringComparison.OrdinalIgnoreCase))
            {
                BotLogger.LogError(clientInfo.Id, $"🚫 'Tutamazsin' waypoint'i tespit edildi! Client #{clientInfo.Id} balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                BringMainFormToFront();
                ShowTutamazsinWarning(clientInfo.Id);
                return;
            }

            // =========================================================================
            // 5. ADIM: Filtre & AutoPass Kontrolü ("Balığı Tut" / "Yakala" Durumu)
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

            // =========================================================================
            // 6. ADIM: FisherManSearchArea içinde FishingMenuTitle şablonunu bekle
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "Fisherman alanında 'FishingMenuTitle' başlığı aranıyor...");

            bool menuTitleFound = false;

            while (!cancellationToken.IsCancellationRequested)
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
                return;

            // =========================================================================
            // 7. ADIM: Karara Göre Eylem (Çıkış Butonu Tıkla VEYA Balık Oyunu Başlat)
            // =========================================================================
            if (isAutoPass || !isCatchEnabled)
            {
                // Çıkış butonuna (FishingMenuExitButtonPosition) git ve tıkla
                int exitLocalX = RegionConstants.FishingMenuExitButtonPosition.StartX + (RegionConstants.FishingMenuExitButtonPosition.Width / 2);
                int exitLocalY = RegionConstants.FishingMenuExitButtonPosition.StartY + (RegionConstants.FishingMenuExitButtonPosition.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"FishingMenuExitButton ({exitLocalX}, {exitLocalY}) alanına tıklanarak menü kapatılıyor...");

                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, exitLocalX, exitLocalY, fastMove: false, cancellationToken: cancellationToken);
                await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "FishingMenuExitButton tıklandı ve menü kapatıldı.");

                // Animasyon İptali Yap
                await PerformAnimationCancelAsync(clientInfo, settings, cancellationToken);

                BotLogger.LogInfo(clientInfo.Id, "Döngü tamamlandı, yeni balık tutma adımına geçiliyor...");
                return;
            }

            // Normal Balık Oyunu: Eşzamanlı Balık Oyunu ve ChatArea Waypoint Taraması
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

            // Balık oyunu bittiğinde Animasyon İptali Yap
            await PerformAnimationCancelAsync(clientInfo, settings, cancellationToken);

            // =========================================================================
            // 8. ADIM: Tutamazsin & YakalananBalik Durumu ve Envanter Boş Slot (EmptySlot) Kontrolü
            // =========================================================================
            if (matchedWaypoint != null && (matchedWaypoint.TemplatePath == TemplateConstants.Waypoints.Tutamazsin || matchedWaypoint.TemplateName.Equals("tutamazsin", StringComparison.OrdinalIgnoreCase)))
            {
                BotLogger.LogError(clientInfo.Id, $"🚫 'Tutamazsin' waypoint'i tespit edildi! Client #{clientInfo.Id} balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                BringMainFormToFront();
                ShowTutamazsinWarning(clientInfo.Id);
                return;
            }

            if (matchedWaypoint != null && (matchedWaypoint.TemplatePath == TemplateConstants.Waypoints.YakalananBalik || matchedWaypoint.TemplateName.Equals("yakalanan_balik", StringComparison.OrdinalIgnoreCase)))
            {
                BotLogger.LogInfo(clientInfo.Id, "🎣 'YakalananBalik' waypoint'i tespit edildi. 100ms bekleniyor ve InventoryFishArea boş slot sayısı kontrol ediliyor...");
                await Task.Delay(100, cancellationToken);

                using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryFishArea))
                {
                    if (fishAreaBmp != null)
                    {
                        var emptySlots = TemplateConstants.MatchAll(fishAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);
                        int emptyCount = emptySlots.Count;

                        BotLogger.LogInfo(clientInfo.Id, $"InventoryFishArea boş slot sayısı: {emptyCount}");

                        if (emptyCount == 0)
                        {
                            BotLogger.LogWarning(clientInfo.Id, "🛑 InventoryFishArea içerisinde boş slot kalmadı (EmptySlot: 0)!");

                            // 1. Pişir seçeneği aktif balık var mı kontrol et ve kamp ateşinde pişir
                            bool cookedAny = await FishCookingFunction.ExecuteCookingProcessAsync(clientInfo, settings, cancellationToken);

                            // 2. Pişirme sonrası güncel boş slot sayısını tekrar kontrol et
                            int finalEmptyCount = 0;
                            using (Bitmap? recheckBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryFishArea))
                            {
                                if (recheckBmp != null)
                                {
                                    var recheckSlots = TemplateConstants.MatchAll(recheckBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);
                                    finalEmptyCount = recheckSlots.Count;
                                }
                            }

                            BotLogger.LogInfo(clientInfo.Id, $"Pişirme sonrası güncel boş slot sayısı: {finalEmptyCount}");

                            if (finalEmptyCount == 0)
                            {
                                BotLogger.LogWarning(clientInfo.Id, "🛑 Boş slot açılamadı veya hiç boş slot kalmadı! Balık botu durduruluyor...");
                                Services.FishBotService.Instance.StopFishBot(clientInfo.Id);
                                BringMainFormToFront();
                                return;
                            }
                            else
                            {
                                BotLogger.LogSuccess(clientInfo.Id, $"🎉 Pişirme işlemiyle {finalEmptyCount} adet boş slot açıldı. Balık tutma döngüsüne devam ediliyor.");
                            }
                        }
                    }
                }
            }

            BotLogger.LogInfo(clientInfo.Id, "Balık tutma döngüsü tamamlandı, yeni olta atma adımına geçiliyor...");
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
    }
}
