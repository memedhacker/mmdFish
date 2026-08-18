using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Envanterdeki pişirilebilir balıkları yerdeki kamp ateşine sürükleyip pişiren modüler fonksiyon sınıfı.
    ///
    /// ALGORİTMA:
    /// A: Tüm slotlarda fareyi gezdir. (HoverAcrossInventoryFishAreaAsync)
    /// B: "Pişir" seçeneği olan balıklar var mı diye kontrol et. (GrilledFishes, DeadFishes, FishIconTemplates matching; en yüksek eşleşme Izgara_ olmayanlar)
    /// C1: Çantada kamp ateşi var mı diye kontrol et.
    /// C2: Kamp ateşi yoksa ve satın al kapalıysa çantadaki boş slotları say.
    /// C3: Boş slot yoksa botu durdur.
    /// C4: Kamp ateşi yoksa ve satın al seçeneği aktifse balıkçıdan ateş satın al.
    /// D: Kamp ateşi varsa rastgele 1 tanesine sağ tıkla.
    /// D2: FisherManSearchArea içerisinde KampAtesiFloor ve KampAtesiFloor2 ara.
    /// D3: Sırayla pişirilecek tüm balıkları sürükle ve ateşin üzerine bırak.
    ///     HER SÜRÜKLEME ÖNCESİNDE ATEŞİN VAR OLDUĞUNU TEYİT ET. EĞER YOKSA C1 ADIMINA GERİ DÖN.
    /// D4: Pişirme sonrası boş slot açıldı mı?
    ///     D4(EVET) -> E Adımına geç (Balık tutmaya devam et).
    ///     D4(HAYIR) -> Botu durdur.
    /// E: Tüm balıklar pişirildikten sonra balık tutma döngüsüne tekrardan başla.
    /// </summary>
    public static class FishCookingFunction
    {
        /// <summary>
        /// Balık pişirme sürecini baştan sona yönetir.
        /// </summary>
        public static async Task<bool> ExecuteCookingProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return false;

            // =========================================================================
            // ADIM A: Tüm slotlarda fareyi gezdir
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Balık Pişirme - Adım A] InventoryFishArea slotları üzerinde fare gezdiriliyor...");
            await FishingExecutionFunction.HoverAcrossInventoryFishAreaAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(Random.Shared.Next(100, 200), cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                // =========================================================================
                // ADIM B: "Pişir" seçeneği olan balıklar var mı diye kontrol et
                // =========================================================================
                BotLogger.LogInfo(clientInfo.Id, "[Balık Pişirme - Adım B] Pişirilebilir balıklar taranıyor...");
                var cookableFish = ScanCookableFishInFishArea(clientInfo.Handle, settings);

                // Eğer pişirilecek balık kalmadıysa:
                if (cookableFish.Count == 0)
                {
                    BotLogger.LogInfo(clientInfo.Id, "[Balık Pişirme] Envanterde 'Pişir' seçeneği aktif balık kalmadı / bulunamadı.");
                    break;
                }

                BotLogger.LogInfo(clientInfo.Id, $"🔥 [Balık Pişirme] Envanterde {cookableFish.Count} adet pişirilecek balık tespit edildi.");

                // =========================================================================
                // ADIM C1: Çantada kamp ateşi var mı diye kontrol et
                // =========================================================================
                BotLogger.LogInfo(clientInfo.Id, "[Balık Pişirme - Adım C1] Envanterdeki kamp ateşleri taranıyor...");
                await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
                var campfires = ScanCampfiresInInventory(clientInfo.Handle);

                // =========================================================================
                // ADIM C2 - C4: Kamp ateşi yoksa kontrol ve satın alma
                // =========================================================================
                if (campfires.Count == 0)
                {
                    if (!settings.BuyCampfireEnabled)
                    {
                        // C2: Kamp ateşi satın alma kapalıysa
                        BotLogger.LogWarning(clientInfo.Id, "⚠️ [Balık Pişirme - Adım C2] Çantada kamp ateşi yok ve 'Kamp Ateşi Satın Al' seçeneği kapalı!");
                        break;
                    }
                    else
                    {
                        // C4: Kamp ateşi satın alma aktifse balıkçıdan ateş satın al
                        BotLogger.LogInfo(clientInfo.Id, "🛒 [Balık Pişirme - Adım C4] Çantada kamp ateşi yok, balıkçıdan kamp ateşi satın alma başlatılıyor...");
                        await StartupFishermanFunction.ExecuteAsync(clientInfo, cancellationToken);
                        await Task.Delay(300, cancellationToken);

                        // Yeniden kamp ateşi kontrolü yap
                        campfires = ScanCampfiresInInventory(clientInfo.Handle);
                        if (campfires.Count == 0)
                        {
                            BotLogger.LogWarning(clientInfo.Id, "🛑 [Balık Pişirme] Balıkçıdan kamp ateşi temin edilemedi!");
                            break;
                        }
                    }
                }

                // =========================================================================
                // ADIM D: Kamp ateşi varsa rastgele 1 tanesine sağ tıkla
                // =========================================================================
                var randomFire = campfires[Random.Shared.Next(campfires.Count)];
                int fireLocalX = RegionConstants.InventoryBaitArea.StartX + randomFire.Location.X + (randomFire.Bounds.Width / 2);
                int fireLocalY = RegionConstants.InventoryBaitArea.StartY + randomFire.Location.Y + (randomFire.Bounds.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"🔥 [Balık Pişirme - Adım D] Kamp ateşine ({fireLocalX}, {fireLocalY}) sağ tıklanarak yakılıyor...");
                await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, fireLocalX, fireLocalY, fastMove: false, cancellationToken: cancellationToken);
                await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);

                await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
                await Task.Delay(Random.Shared.Next(150, 300), cancellationToken);

                // =========================================================================
                // ADIM D2: FisherManSearchArea içerisinde KampAtesiFloor / 2 ara
                // =========================================================================
                Point? floorFireLoc = FindFloorCampfireLocation(clientInfo.Handle);
                if (floorFireLoc == null)
                {
                    await Task.Delay(400, cancellationToken);
                    floorFireLoc = FindFloorCampfireLocation(clientInfo.Handle);
                }

                if (floorFireLoc == null)
                {
                    BotLogger.LogWarning(clientInfo.Id, "⚠️ [Balık Pişirme - Adım D2] Yerde kurulan kamp ateşi tespit edilemedi! C1 adımına geri dönülüyor...");
                    continue;
                }

                BotLogger.LogSuccess(clientInfo.Id, $"🔥 [Balık Pişirme - Adım D2] Yerdeki kamp ateşi tespit edildi ({floorFireLoc.Value.X}, {floorFireLoc.Value.Y}).");

                // =========================================================================
                // ADIM D3: Sırayla pişirilecek tüm balıkları sürükle ve ateşin üzerine bırak
                // HER SÜRÜKLEME ÖNCESİNDE ATEŞİN VAR OLDUĞUNU TEYİT ET!
                // =========================================================================
                bool fireExtinguished = false;
                for (int i = 0; i < cookableFish.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    // Her sürükleme öncesinde yerdeki ateşin varlığını teyit et
                    Point? currentFloorFire = FindFloorCampfireLocation(clientInfo.Handle);
                    if (currentFloorFire == null)
                    {
                        BotLogger.LogWarning(clientInfo.Id, "⚠️ [Balık Pişirme - Adım D3] Yerdeki kamp ateşi söndü! Kalan balıklar için C1 adımına dönülüyor...");
                        fireExtinguished = true;
                        break;
                    }

                    var fish = cookableFish[i];
                    int fishLocalX = RegionConstants.InventoryFishArea.StartX + fish.Location.X + (fish.Bounds.Width / 2);
                    int fishLocalY = RegionConstants.InventoryFishArea.StartY + fish.Location.Y + (fish.Bounds.Height / 2);

                    BotLogger.LogInfo(clientInfo.Id, $"[{i + 1}/{cookableFish.Count}] '{fish.TemplateName}' ({fishLocalX}, {fishLocalY}) yerdeki ateşe ({currentFloorFire.Value.X}, {currentFloorFire.Value.Y}) sürükleniyor...");
                    await HumanMouseService.Instance.DragAndDropLocalAsync(
                        clientInfo.Handle,
                        fishLocalX,
                        fishLocalY,
                        currentFloorFire.Value.X,
                        currentFloorFire.Value.Y,
                        fastMove: false,
                        cancellationToken: cancellationToken);

                    await Task.Delay(Random.Shared.Next(250, 400), cancellationToken);
                }

                if (fireExtinguished)
                {
                    continue;
                }

                // Tüm balıklar sürüklendi
                break;
            }

            // Fareyi dışarı çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(150, cancellationToken);

            // =========================================================================
            // ADIM D4: PİŞİRME SONRASI BOŞ SLOT AÇILDI MI?
            // =========================================================================
            int finalEmptyCount = FishingExecutionFunction.ScanEmptySlots(clientInfo.Handle);
            BotLogger.LogInfo(clientInfo.Id, $"[Balık Pişirme - Adım D4] Pişirme sonrası güncel boş slot sayısı: {finalEmptyCount}");

            if (finalEmptyCount == 0)
            {
                // D4 (HAYIR) -> BOTU DURDUR & MainForm Öne Getir
                BotLogger.LogWarning(clientInfo.Id, "🛑 [Balık Pişirme - Adım D4] Pişirme sonrası hiç boş slot açılamadı (EmptySlot: 0)! Balık botu durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }

            // D4 (EVET) -> E ADIMINA GEÇ (Balık tutma döngüsüne devam et)
            BotLogger.LogSuccess(clientInfo.Id, $"🎉 [Balık Pişirme - Adım E] {finalEmptyCount} adet boş slot açıldı. Balık tutma döngüsüne devam ediliyor.");
            return true;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde TÜM balık şablonlarını tarar.
        /// Slot bazında en yüksek benzerliğe sahip şablonu belirler (NMS).
        /// En yüksek eşleşmesi Izgara_ OLMAYAN (yani FishIconTemplates veya DeadFishes) ve
        /// ayarlarında "Pişir" seçeneği işaretli olan balıkları döndürür.
        /// </summary>
        public static List<TemplateMatchResult> ScanCookableFishInFishArea(IntPtr hWnd, FishBotSettings settings)
        {
            var cookableMatches = new List<TemplateMatchResult>();
            if (settings == null || settings.FishFilter == null) return cookableMatches;

            var allFishTemplates = FishKillingFunction.GetAllFishTemplates();
            if (allFishTemplates.Count == 0) return cookableMatches;

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return cookableMatches;

                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, allFishTemplates, threshold: 0.80, useGrayscale: false);
                if (allFound == null || allFound.Count == 0) return cookableMatches;

                allFound.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

                var bestSlotMatches = new List<TemplateMatchResult>();
                foreach (var m in allFound)
                {
                    bool isOverlapping = bestSlotMatches.Any(existing =>
                    {
                        int overlapThresholdX = Math.Max(10, Math.Min(existing.Bounds.Width, m.Bounds.Width) / 2);
                        int overlapThresholdY = Math.Max(10, Math.Min(existing.Bounds.Height, m.Bounds.Height) / 2);
                        return Math.Abs(existing.Location.X - m.Location.X) < overlapThresholdX &&
                               Math.Abs(existing.Location.Y - m.Location.Y) < overlapThresholdY;
                    });

                    if (!isOverlapping)
                    {
                        bestSlotMatches.Add(m);
                    }
                }

                // Filtreleme: Yalnızca en iyi eşleşmesi Izgara_ OLMAYAN ve "Pişir" işaretli olanlar
                foreach (var slotMatch in bestSlotMatches)
                {
                    string rawName = Path.GetFileNameWithoutExtension(slotMatch.TemplatePath);

                    bool isGrilled = rawName.StartsWith("Izgara_", StringComparison.OrdinalIgnoreCase) ||
                                     slotMatch.TemplatePath.Contains("Izgara_", StringComparison.OrdinalIgnoreCase);

                    if (!isGrilled)
                    {
                        if (IsFishCheckedInFilter(settings, rawName, "Pişir"))
                        {
                            cookableMatches.Add(slotMatch);
                        }
                    }
                }
            }

            return cookableMatches;
        }

        /// <summary>
        /// InventoryBaitArea alanında kamp ateşi (ates.png) şablonlarını tarar.
        /// </summary>
        public static List<TemplateMatchResult> ScanCampfiresInInventory(IntPtr hWnd)
        {
            var results = new List<TemplateMatchResult>();
            using (Bitmap? baitAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryBaitArea))
            {
                if (baitAreaBmp == null) return results;

                var matches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.75);
                matches.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

                foreach (var m in matches)
                {
                    if (!results.Any(existing => Math.Abs(existing.Location.X - m.Location.X) < 16 && Math.Abs(existing.Location.Y - m.Location.Y) < 16))
                    {
                        results.Add(m);
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// FisherManSearchArea içerisinde yerdeki kamp ateşini (KampAtesiFloor / KampAtesiFloor2) arar.
        /// </summary>
        private static Point? FindFloorCampfireLocation(IntPtr hWnd)
        {
            using (Bitmap? searchBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.FisherManSearchArea))
            {
                if (searchBmp == null) return null;

                var floorTemplates = new[]
                {
                    TemplateConstants.Fisherman.KampAtesiFloor,
                    TemplateConstants.Fisherman.KampAtesiFloor2
                };

                var match = TemplateConstants.FindBestMatch(searchBmp, floorTemplates, minThreshold: 0.60);
                if (match != null && match.IsSuccess)
                {
                    int localX = RegionConstants.FisherManSearchArea.StartX + match.Location.X + (match.Bounds.Width / 2);
                    int localY = RegionConstants.FisherManSearchArea.StartY + match.Location.Y + (match.Bounds.Height / 2);
                    return new Point(localX, localY);
                }
            }
            return null;
        }

        /// <summary>
        /// Ayarlarda belirtilen balık adı için ilgili sütunun (Örn: "Pişir") seçili olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsFishCheckedInFilter(FishBotSettings settings, string fishName, string checkColumnName)
        {
            if (settings == null || settings.FishFilter == null) return false;

            string baseFishName = fishName.Replace("Ölü_", "").Replace("Izgara_", "").Trim();

            foreach (var category in settings.FishFilter.Values)
            {
                if (category.TryGetValue(fishName, out var filterItem) || category.TryGetValue(baseFishName, out filterItem))
                {
                    if (filterItem.GetCheck(checkColumnName, false))
                    {
                        return true;
                    }
                }
            }

            string normBase = NormalizeKey(baseFishName);
            foreach (var category in settings.FishFilter.Values)
            {
                foreach (var kvp in category)
                {
                    if (NormalizeKey(kvp.Key) == normBase)
                    {
                        if (kvp.Value.GetCheck(checkColumnName, false))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Türkçe karakterleri ve ayraçları temizleyerek anahtar normalizasyonu yapar.
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
    }
}
