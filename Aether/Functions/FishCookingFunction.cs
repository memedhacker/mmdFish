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
    /// A: InventoryFishArea içerisinde pişirme seçeneği aktif olan bir balık var mı diye kontrol et.
    ///    [Templateler içerisinde Izgara dışındaki balıkları dahil et sadece. FishIconTemplates ve DeadFishes içerisinden].
    /// B: Pişirilmeye uygun balık(lar) varsa InventoryBaitArea içerisinden herhangi bir Ates e sağ tıkla.[ardından 100ms bekle]
    /// B2: EĞER PİŞİRİLMEYE UYGUN BALIK YOKSA BOTU DURDUR
    /// C: FisherManSearchArea içerisinde KampAtesiFloor ve KampAtesiFloor2 templatelerini ara.[Bulduğunda konumuyla beraber D adımına geç]
    /// D: Pişirilmeye uygun balıkları sırayla FisherManSearchArea içerisinde bulduğun ateş konumuna sürükle.
    /// E: Tüm balıklar piştiğinde tekrardan InventoryFishArea'da boş alan kontrolü adımına dön ve boş yer varsa balık tutmaya devam et.
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
            // ADIM A: InventoryFishArea içerisinde pişirme seçeneği aktif olan balıkları belirle ve tara
            // =========================================================================
            var cookableTemplates = GetCookableTemplates(settings);
            if (cookableTemplates.Count == 0)
            {
                BotLogger.LogWarning(clientInfo.Id, "🛑 FishFilter ayarlarında 'Pişir' seçeneği aktif olan hiçbir balık bulunmuyor! Bot durduruluyor...");
                FishBotService.Instance.StopFishBot(clientInfo.Id);
                FishingExecutionFunction.BringMainFormToFront();
                return false;
            }

            List<TemplateMatchResult> fishToCook = ScanCookableFishInFishArea(clientInfo.Handle, cookableTemplates);

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
                fishToCook = ScanCookableFishInFishArea(clientInfo.Handle, cookableTemplates);
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
            var cookableTemplates = GetCookableTemplates(settings);
            if (cookableTemplates.Count == 0) return false;

            var fish = ScanCookableFishInFishArea(hWnd, cookableTemplates);
            return fish.Count > 0;
        }

        /// <summary>
        /// FishFilter ayarlarında "Pişir" seçeneği işaretli olan şablon yollarını döndürür.
        /// [Izgara DAHİL EDİLMEZ. Yalnızca Common, Rare ve DeadFishes içerisinden alınır].
        /// </summary>
        public static List<string> GetCookableTemplates(FishBotSettings settings)
        {
            var cookable = new List<string>();
            if (settings == null || settings.FishFilter == null) return cookable;

            // Izgara DAHİL EDİLMEZ. Yalnızca FishIconTemplates (Common, Rare) ve DeadFishes şablonları alınır.
            var candidatePool = new List<string>();
            candidatePool.AddRange(TemplateConstants.FishIconTemplates.Common.All);
            candidatePool.AddRange(TemplateConstants.FishIconTemplates.Rare.All);
            candidatePool.AddRange(TemplateConstants.FishIconTemplates.DeadFishes.All);

            foreach (var templatePath in candidatePool)
            {
                string rawItemName = Path.GetFileNameWithoutExtension(templatePath);
                // "Ölü_Levrek" -> "Levrek" fallback'i için temiz isim
                string baseItemName = rawItemName.StartsWith("Ölü_", StringComparison.OrdinalIgnoreCase)
                    ? rawItemName.Substring(4)
                    : rawItemName;

                bool isCook = false;

                // 1. Doğrudan sözlük kontrolü
                foreach (var category in settings.FishFilter.Values)
                {
                    if (category.TryGetValue(rawItemName, out var filterItem) ||
                        category.TryGetValue(baseItemName, out filterItem))
                    {
                        if (filterItem.GetCheck("Pişir", false))
                        {
                            isCook = true;
                            break;
                        }
                    }
                }

                // 2. Normalize edilmiş anahtarla kontrol (Fallback)
                if (!isCook)
                {
                    string normRaw = NormalizeKey(rawItemName);
                    string normBase = NormalizeKey(baseItemName);

                    foreach (var category in settings.FishFilter.Values)
                    {
                        foreach (var kvp in category)
                        {
                            string normKey = NormalizeKey(kvp.Key);
                            if (normKey == normRaw || normKey == normBase)
                            {
                                if (kvp.Value.GetCheck("Pişir", false))
                                {
                                    isCook = true;
                                    break;
                                }
                            }
                        }
                        if (isCook) break;
                    }
                }

                if (isCook)
                {
                    cookable.Add(templatePath);
                }
            }

            return cookable;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde pişirilmesi gereken balıkları (Common, Rare, DeadFishes) tarar.
        /// Aynı pozisyonda birden fazla şablon eşleşmesi varsa benzerlik skoru en yüksek olanı seçer ve çakışmaları eler.
        /// </summary>
        public static List<TemplateMatchResult> ScanCookableFishInFishArea(IntPtr hWnd, List<string> cookableTemplates)
        {
            var matches = new List<TemplateMatchResult>();
            if (cookableTemplates == null || cookableTemplates.Count == 0) return matches;

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return matches;

                // useGrayscale: false ile renk korumalı şablon eşleştirme
                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, cookableTemplates, threshold: 0.80, useGrayscale: false);
                if (allFound == null || allFound.Count == 0) return matches;

                // 1. En yüksek benzerlik puanına (Confidence) göre azalan sırada sırala
                allFound.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

                // 2. Aynı / çakışan pozisyondaki eşleşmelerden yalnızca en yüksek benzerliğe sahip olanı tut (NMS)
                foreach (var m in allFound)
                {
                    bool isOverlapping = matches.Any(existing =>
                    {
                        int overlapThresholdX = Math.Max(10, Math.Min(existing.Bounds.Width, m.Bounds.Width) / 2);
                        int overlapThresholdY = Math.Max(10, Math.Min(existing.Bounds.Height, m.Bounds.Height) / 2);
                        return Math.Abs(existing.Location.X - m.Location.X) < overlapThresholdX &&
                               Math.Abs(existing.Location.Y - m.Location.Y) < overlapThresholdY;
                    });

                    if (!isOverlapping)
                    {
                        matches.Add(m);
                    }
                }
            }

            return matches;
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
