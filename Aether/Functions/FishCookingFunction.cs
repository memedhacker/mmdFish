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
    /// Envanterdeki pişirilebilir balıkları kamp ateşinde pişiren modüler fonksiyon sınıfı.
    ///
    /// ALGORİTMA:
    /// - Tüm balık şablonları (Normal, Ölü, Izgara) ile InventoryFishArea taranır.
    /// - Aynı pozisyondaki en yüksek benzerliğe sahip şablon seçilir (NMS).
    /// - En yüksek eşleşmesi Izgara_ OLMAYAN (yani Ölü_ veya Normal) balıklar belirlenir.
    /// - Bu balıklardan ayarlarında "Pişir" seçeneği aktif olanlar sırayla kamp ateşine sürüklenir.
    /// - Tüm balıklar piştiğinde envanterde boş slot açılıp açılmadığı kontrol edilir.
    /// </summary>
    public static class FishCookingFunction
    {
        /// <summary>
        /// Pişirme sürecini baştan sona yönetir:
        /// </summary>
        public static async Task<bool> ExecuteCookingProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return false;

            // =========================================================================
            // ADIM A: Tüm balık şablonlarını tara ve pişirilecek balıkları filtrele
            // =========================================================================
            List<TemplateMatchResult> fishToCook = ScanCookableFishInFishArea(clientInfo.Handle, settings);

            // =========================================================================
            // ADIM B2: EĞER PİŞİRİLMEYE UYGUN BALIK YOKSA BOTU DURDUR
            // =========================================================================
            if (fishToCook.Count == 0)
            {
                BotLogger.LogWarning(clientInfo.Id, "🛑 InventoryFishArea içerisinde pişirilecek uygun balık bulunamadı! Bot durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }

            BotLogger.LogInfo(clientInfo.Id, $"🔥 Envanterde {fishToCook.Count} adet pişirilebilir balık tespit edildi.");

            // =========================================================================
            // ADIM B: InventoryBaitArea içerisinden herhangi bir Ates'e sağ tıkla [ardından 100ms bekle]
            // =========================================================================
            TemplateMatchResult? fireMatch = FindCampfireInBaitArea(clientInfo.Handle);
            if (fireMatch == null || !fireMatch.IsSuccess)
            {
                BotLogger.LogWarning(clientInfo.Id, "⚠️ InventoryBaitArea içerisinde kamp ateşi (ates.png) bulunamadı! InventoryPosition genelinde aranıyor...");
                fireMatch = FindCampfireInInventory(clientInfo.Handle);
            }

            if (fireMatch == null || !fireMatch.IsSuccess)
            {
                BotLogger.LogError(clientInfo.Id, "❌ Envanterde kamp ateşi (ates.png) bulunamadı! Pişirme yapılamıyor. Bot durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }

            int fireLocalX = fireMatch.Location.X + (fireMatch.Bounds.Width / 2);
            int fireLocalY = fireMatch.Location.Y + (fireMatch.Bounds.Height / 2);

            BotLogger.LogInfo(clientInfo.Id, $"Kamp ateşine ({fireLocalX}, {fireLocalY}) sağ tıklanarak yere kuruluyor...");
            await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, fireLocalX, fireLocalY, fastMove: false, cancellationToken: cancellationToken);

            // Ardından 100ms bekle
            await Task.Delay(100, cancellationToken);

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // =========================================================================
            // ADIM C: FisherManSearchArea içerisinde KampAtesiFloor ve KampAtesiFloor2 templatelerini ara
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "FisherManSearchArea içerisinde yerdeki kamp ateşi (KampAtesiFloor / KampAtesiFloor2) aranıyor...");
            TemplateMatchResult? floorFireMatch = await WaitForFloorCampfireAsync(clientInfo.Handle, cancellationToken);

            if (floorFireMatch == null)
            {
                BotLogger.LogError(clientInfo.Id, "❌ Yerdeki Kamp Ateşi tespit edilemedi! Pişirme iptal ediliyor. Bot durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }

            int fireTargetX = floorFireMatch.Location.X;
            int fireTargetY = floorFireMatch.Location.Y;
            BotLogger.LogSuccess(clientInfo.Id, $"🔥 Yerdeki Kamp Ateşi tespit edildi! Konum: ({fireTargetX}, {fireTargetY}), Benzerlik: %{floorFireMatch.Confidence * 100:F1}");

            // =========================================================================
            // ADIM D: Pişirilmeye uygun balıkları sırayla bulduğun ateş konumuna sürükle
            // =========================================================================
            int totalCooked = 0;
            const int maxIterations = 40;
            int iteration = 0;

            while (iteration < maxIterations && !cancellationToken.IsCancellationRequested)
            {
                iteration++;

                // Envanterdeki pişirilebilir balıkları tara
                fishToCook = ScanCookableFishInFishArea(clientInfo.Handle, settings);
                if (fishToCook.Count == 0)
                {
                    BotLogger.LogSuccess(clientInfo.Id, $"✅ Pişirilecek başka balık kalmadı. Toplam {totalCooked} adet balık pişirildi.");
                    break;
                }

                var currentFish = fishToCook[0];
                int fishLocalX = RegionConstants.InventoryFishArea.StartX + currentFish.Location.X + (currentFish.Bounds.Width / 2);
                int fishLocalY = RegionConstants.InventoryFishArea.StartY + currentFish.Location.Y + (currentFish.Bounds.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"[{totalCooked + 1}] '{currentFish.TemplateName}' ({fishLocalX}, {fishLocalY}) -> Kamp Ateşi ({fireTargetX}, {fireTargetY}) sürükleniyor...");

                // Balığı kamp ateşine sürükle ve bırak (Drag & Drop)
                await HumanMouseService.Instance.DragAndDropLocalAsync(
                    clientInfo.Handle,
                    fishLocalX,
                    fishLocalY,
                    fireTargetX,
                    fireTargetY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                totalCooked++;
                await Task.Delay(Random.Shared.Next(300, 450), cancellationToken);
            }

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(200, cancellationToken);

            // =========================================================================
            // ADIM E: Tüm balıklar piştiğinde InventoryFishArea'da boş alan kontrolü yap
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Adım E] Pişirme tamamlandı. InventoryFishArea boş slot sayısı kontrol ediliyor...");

            int emptyCount = 0;
            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp != null)
                {
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
                    emptyCount = uniqueEmpty.Count;
                }
            }

            BotLogger.LogInfo(clientInfo.Id, $"[Adım E] Güncel boş slot sayısı: {emptyCount}");

            if (emptyCount > 0)
            {
                BotLogger.LogSuccess(clientInfo.Id, $"🎉 Pişirme işlemiyle {emptyCount} adet boş slot açıldı. Balık tutma döngüsüne devam ediliyor.");
                return true;
            }
            else
            {
                BotLogger.LogWarning(clientInfo.Id, "🛑 Pişirme sonrası InventoryFishArea içerisinde boş yer açılamadı! Bot durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }
        }

        /// <summary>
        /// InventoryFishArea içerisinde ayarlar doğrultusunda pişirilebilecek balık olup olmadığını kontrol eder.
        /// </summary>
        public static bool HasCookableFish(IntPtr hWnd, int clientId, FishBotSettings settings)
        {
            var fish = ScanCookableFishInFishArea(hWnd, settings);
            return fish.Count > 0;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde TÜM balık şablonlarını (Normal, Ölü, Izgara) tarar.
        /// Slot bazında en yüksek benzerliğe sahip şablonu belirler (NMS).
        /// Sadece en yüksek eşleşmesi Izgara_ OLMAYAN (yani Ölü_ veya Normal) ve
        /// ayarlarında "Pişir" seçeneği işaretli olan balıkları döndürür.
        /// </summary>
        public static List<TemplateMatchResult> ScanCookableFishInFishArea(IntPtr hWnd, FishBotSettings settings)
        {
            var cookableMatches = new List<TemplateMatchResult>();
            if (settings == null || settings.FishFilter == null) return cookableMatches;

            var allFishTemplates = GetAllFishTemplates();
            if (allFishTemplates.Count == 0) return cookableMatches;

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return cookableMatches;

                // TÜM balık şablonları ile renk korumalı arama
                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, allFishTemplates, threshold: 0.80, useGrayscale: false);
                if (allFound == null || allFound.Count == 0) return cookableMatches;

                // 1. En yüksek benzerlik puanına (Confidence) göre azalan sırada sırala
                allFound.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

                // 2. Slot bazında en iyi eşleşmeyi seç (Non-Maximum Suppression)
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

                // 3. Filtreleme: Yalnızca en iyi eşleşmesi Izgara_ OLMAYAN (Ölü veya Normal) ve "Pişir" işaretli olanları seç
                foreach (var slotMatch in bestSlotMatches)
                {
                    string rawName = Path.GetFileNameWithoutExtension(slotMatch.TemplatePath);

                    bool isGrilled = rawName.StartsWith("Izgara_", StringComparison.OrdinalIgnoreCase) ||
                                     slotMatch.TemplatePath.Contains("Izgara_", StringComparison.OrdinalIgnoreCase);

                    // Izgara_ olanlar zaten pişmiştir, elenir
                    if (isGrilled) continue;

                    string baseFishName = rawName.StartsWith("Ölü_", StringComparison.OrdinalIgnoreCase)
                        ? rawName.Substring(4)
                        : rawName;

                    if (IsFishCheckedInFilter(settings, baseFishName, "Pişir") || IsFishCheckedInFilter(settings, rawName, "Pişir"))
                    {
                        cookableMatches.Add(slotMatch);
                    }
                }
            }

            return cookableMatches;
        }

        /// <summary>
        /// Karşılaştırma ve doğru sınıflandırma için TÜM balık şablonlarını (Normal, Ölü, Izgara) döndürür.
        /// </summary>
        public static List<string> GetAllFishTemplates()
        {
            var list = new List<string>();
            list.AddRange(TemplateConstants.FishIconTemplates.Common.All);
            list.AddRange(TemplateConstants.FishIconTemplates.Rare.All);
            list.AddRange(TemplateConstants.FishIconTemplates.DeadFishes.All);
            list.AddRange(TemplateConstants.FishIconTemplates.GrilledFishes.All);
            return list;
        }

        /// <summary>
        /// Ayarlarda belirtilen balık adı için ilgili sütunun (Örn: "Pişir") seçili olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsFishCheckedInFilter(FishBotSettings settings, string fishName, string checkColumnName)
        {
            if (settings == null || settings.FishFilter == null) return false;

            // 1. Doğrudan sözlük kontrolü
            foreach (var category in settings.FishFilter.Values)
            {
                if (category.TryGetValue(fishName, out var filterItem))
                {
                    if (filterItem.GetCheck(checkColumnName, false))
                    {
                        return true;
                    }
                }
            }

            // 2. Normalize edilmiş anahtarla kontrol (Fallback)
            string normBase = NormalizeKey(fishName);
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
        /// InventoryBaitArea bölgesinde 'ates.png' şablonunu arar ve en yüksek benzerlikteki eşleşmenin yerel koordinatıyla döner.
        /// </summary>
        public static TemplateMatchResult? FindCampfireInBaitArea(IntPtr hWnd)
        {
            using (Bitmap? baitBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryBaitArea))
            {
                if (baitBmp == null) return null;

                var matches = TemplateConstants.MatchAll(baitBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.70, useGrayscale: false);
                if (matches.Count > 0)
                {
                    // En yüksek benzerlik puanına göre sırala
                    matches.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
                    var m = matches[0];
                    int localX = RegionConstants.InventoryBaitArea.StartX + m.Location.X;
                    int localY = RegionConstants.InventoryBaitArea.StartY + m.Location.Y;

                    return new TemplateMatchResult
                    {
                        IsSuccess = true,
                        TemplatePath = m.TemplatePath,
                        TemplateName = m.TemplateName,
                        Confidence = m.Confidence,
                        Location = new Point(localX, localY),
                        Bounds = new Rectangle(localX, localY, m.Bounds.Width, m.Bounds.Height)
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// InventoryPosition genel bölgesinde 'ates.png' şablonunu arar ve en yüksek benzerlikteki eşleşmeyi döner.
        /// </summary>
        public static TemplateMatchResult? FindCampfireInInventory(IntPtr hWnd)
        {
            using (Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryPosition))
            {
                if (invBmp == null) return null;

                var matches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.70, useGrayscale: false);
                if (matches.Count > 0)
                {
                    // En yüksek benzerlik puanına göre sırala
                    matches.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
                    var m = matches[0];
                    int localX = RegionConstants.InventoryPosition.StartX + m.Location.X;
                    int localY = RegionConstants.InventoryPosition.StartY + m.Location.Y;

                    return new TemplateMatchResult
                    {
                        IsSuccess = true,
                        TemplatePath = m.TemplatePath,
                        TemplateName = m.TemplateName,
                        Confidence = m.Confidence,
                        Location = new Point(localX, localY),
                        Bounds = new Rectangle(localX, localY, m.Bounds.Width, m.Bounds.Height)
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// FisherManSearchArea bölgesinde KampAtesiFloor ve KampAtesiFloor2 şablonlarını (>= %60) arar ve merkez pencere koordinatını döner.
        /// </summary>
        public static TemplateMatchResult? FindFloorCampfire(IntPtr hWnd)
        {
            using (Bitmap? searchBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.FisherManSearchArea))
            {
                if (searchBmp == null) return null;

                var floorCandidates = new[]
                {
                    TemplateConstants.Fisherman.KampAtesiFloor,
                    TemplateConstants.Fisherman.KampAtesiFloor2
                };

                var match = TemplateConstants.FindBestMatch(searchBmp, floorCandidates, minThreshold: 0.60);
                if (match != null && match.IsSuccess && match.Confidence >= 0.60)
                {
                    int localX = RegionConstants.FisherManSearchArea.StartX + match.Location.X + (match.Bounds.Width / 2);
                    int localY = RegionConstants.FisherManSearchArea.StartY + match.Location.Y + (match.Bounds.Height / 2);

                    return new TemplateMatchResult
                    {
                        IsSuccess = true,
                        TemplatePath = match.TemplatePath,
                        TemplateName = match.TemplateName,
                        Confidence = match.Confidence,
                        Location = new Point(localX, localY),
                        Bounds = new Rectangle(localX - (match.Bounds.Width / 2), localY - (match.Bounds.Height / 2), match.Bounds.Width, match.Bounds.Height)
                    };
                }
            }
            return null;
        }

        /// <summary>
        /// Yere kamp ateşi kurulduktan sonra görünmesini belirli bir süre bekler.
        /// </summary>
        public static async Task<TemplateMatchResult?> WaitForFloorCampfireAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            const int maxAttempts = 15;
            const int delayBetweenAttemptsMs = 200;

            for (int i = 1; i <= maxAttempts; i++)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                var match = FindFloorCampfire(hWnd);
                if (match != null)
                {
                    return match;
                }

                await Task.Delay(delayBetweenAttemptsMs, cancellationToken);
            }

            return null;
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
