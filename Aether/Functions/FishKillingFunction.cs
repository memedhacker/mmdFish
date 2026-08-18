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
    /// Envanterdeki öldürülebilir canlı balıkları sağ tıklayarak öldüren modüler fonksiyon sınıfı.
    ///
    /// ALGORİTMA:
    /// - Tüm balık şablonları (Normal, Ölü, Izgara) ile InventoryFishArea taranır.
    /// - Aynı pozisyondaki en yüksek benzerliğe sahip şablon seçilir (NMS).
    /// - En yüksek eşleşmesi Izgara_ veya Ölü_ OLMAYAN (yani Normal Canlı) balıklar belirlenir.
    /// - Bu balıklardan ayarlarında "Öldür" seçeneği aktif olanlar sırayla sağ tıklanarak öldürülür.
    /// - Tüm balıklar bittiğinde fare InventoryFishArea sol dışına çekilir, 5x7 ızgara gezdirilir ve pişirme adımına geçilir.
    /// </summary>
    public static class FishKillingFunction
    {
        /// <summary>
        /// Öldürme sürecini baştan sona yönetir.
        /// </summary>
        public static async Task ExecuteKillingProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return;

            // =========================================================================
            // BAŞLANGIÇ: 5x7 slotlarda fareyi gezdir (Slot taraması/hover)
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Balık Öldürme] Başlangıçta InventoryFishArea 5x7 slotları üzerinde fare gezdiriliyor...");
            await FishingExecutionFunction.HoverAcrossInventoryFishAreaAsync(clientInfo.Handle, cancellationToken);

            // =========================================================================
            // ADIM A: Tüm balık şablonlarını tara ve öldürülecek NORMAL canlı balıkları filtrele
            // =========================================================================
            var killableFish = ScanKillableFishInFishArea(clientInfo.Handle, settings);

            // =========================================================================
            // ADIM C: EĞER ÖLDÜRÜLMEYE UYGUN BALIK YOKSA PİŞİRME ADIMINA GEÇ
            // =========================================================================
            if (killableFish.Count == 0)
            {
                BotLogger.LogInfo(clientInfo.Id, "[Balık Öldürme] InventoryFishArea içerisinde öldürülecek uygun canlı balık bulunamadı. Pişirme adımına geçiliyor.");
                return;
            }

            BotLogger.LogInfo(clientInfo.Id, $"⚔️ [Balık Öldürme] Envanterde {killableFish.Count} adet öldürülecek canlı balık tespit edildi.");

            // =========================================================================
            // ADIM B: Öldürmeye uygun balıklara sırayla sağ tıkla
            // =========================================================================
            int totalKilled = 0;
            const int maxIterations = 40;
            int iteration = 0;

            while (iteration < maxIterations && !cancellationToken.IsCancellationRequested)
            {
                iteration++;

                killableFish = ScanKillableFishInFishArea(clientInfo.Handle, settings);
                if (killableFish.Count == 0)
                {
                    BotLogger.LogSuccess(clientInfo.Id, $"✅ [Balık Öldürme] Öldürülecek başka canlı balık kalmadı. Toplam {totalKilled} adet balık öldürüldü.");
                    break;
                }

                var currentFish = killableFish[0];
                int fishLocalX = RegionConstants.InventoryFishArea.StartX + currentFish.Location.X + (currentFish.Bounds.Width / 2);
                int fishLocalY = RegionConstants.InventoryFishArea.StartY + currentFish.Location.Y + (currentFish.Bounds.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"[{totalKilled + 1}] '{currentFish.TemplateName}' ({fishLocalX}, {fishLocalY}) öldürmek için sağ tıklanıyor...");

                // Balığa sağ tıkla
                await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, fishLocalX, fishLocalY, fastMove: false, cancellationToken: cancellationToken);
                totalKilled++;

                await Task.Delay(Random.Shared.Next(200, 350), cancellationToken);
            }

            // =========================================================================
            // Fareyi InventoryFishArea dışarısında solda herhangi bir noktaya çek
            // =========================================================================
            int outsideX = Math.Max(20, RegionConstants.InventoryFishArea.StartX - Random.Shared.Next(35, 90));
            int outsideY = Random.Shared.Next(RegionConstants.InventoryFishArea.StartY, RegionConstants.InventoryFishArea.EndY);
            BotLogger.LogInfo(clientInfo.Id, $"Fare InventoryFishArea sol dışına ({outsideX}, {outsideY}) çekiliyor...");
            await HumanMouseService.Instance.MoveMouseToLocalAsync(clientInfo.Handle, outsideX, outsideY, cancellationToken);
            await Task.Delay(Random.Shared.Next(100, 200), cancellationToken);

            // =========================================================================
            // ADIM E: Tüm balıklar öldüğünde fareyi tüm InventoryFishArea içerisinde yukarı aşağı gezdir
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Adım E] Balık öldürme işlemi tamamlandı. InventoryFishArea 5x7 slotları taranıyor...");
            await FishingExecutionFunction.HoverAcrossInventoryFishAreaAsync(clientInfo.Handle, cancellationToken);

            // Gezme sonrasında fareyi tekrar InventoryFishArea dışarısında solda güvenli bir noktaya çek
            outsideX = Math.Max(20, RegionConstants.InventoryFishArea.StartX - Random.Shared.Next(35, 90));
            outsideY = Random.Shared.Next(RegionConstants.InventoryFishArea.StartY, RegionConstants.InventoryFishArea.EndY);
            await HumanMouseService.Instance.MoveMouseToLocalAsync(clientInfo.Handle, outsideX, outsideY, cancellationToken);
            await Task.Delay(Random.Shared.Next(80, 150), cancellationToken);

            BotLogger.LogSuccess(clientInfo.Id, "⚔️ Balık öldürme adımı tamamlandı, pişirme sürecine geçiliyor.");
        }

        /// <summary>
        /// InventoryFishArea içerisinde 'Öldür' seçeneği işaretli canlı balık olup olmadığını kontrol eder.
        /// </summary>
        public static bool HasKillableFish(IntPtr hWnd, int clientId, FishBotSettings settings)
        {
            var fish = ScanKillableFishInFishArea(hWnd, settings);
            return fish.Count > 0;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde TÜM balık şablonlarını (Normal, Ölü, Izgara) tarar.
        /// Slot bazında en yüksek benzerliğe sahip şablonu belirler (NMS).
        /// Sadece en yüksek eşleşmesi Izgara_ veya Ölü_ OLMAYAN (yani Normal Canlı) ve
        /// ayarlarında "Öldür" seçeneği işaretli olan balıkları döndürür.
        /// </summary>
        public static List<TemplateMatchResult> ScanKillableFishInFishArea(IntPtr hWnd, FishBotSettings settings)
        {
            var killableMatches = new List<TemplateMatchResult>();
            if (settings == null || settings.FishFilter == null) return killableMatches;

            var allFishTemplates = GetAllFishTemplates();
            if (allFishTemplates.Count == 0) return killableMatches;

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return killableMatches;

                // TÜM balık şablonları ile renk korumalı arama
                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, allFishTemplates, threshold: 0.80, useGrayscale: false);
                if (allFound == null || allFound.Count == 0) return killableMatches;

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

                // 3. Filtreleme: Yalnızca en iyi eşleşmesi Izgara_ veya Ölü_ OLMAYAN (Normal Canlı) ve "Öldür" işaretli olanları seç
                foreach (var slotMatch in bestSlotMatches)
                {
                    string rawName = Path.GetFileNameWithoutExtension(slotMatch.TemplatePath);

                    bool isGrilled = rawName.StartsWith("Izgara_", StringComparison.OrdinalIgnoreCase) ||
                                     slotMatch.TemplatePath.Contains("Izgara_", StringComparison.OrdinalIgnoreCase);

                    bool isDead = rawName.StartsWith("Ölü_", StringComparison.OrdinalIgnoreCase) ||
                                  slotMatch.TemplatePath.Contains("Ölü_", StringComparison.OrdinalIgnoreCase);

                    // Sadece en yüksek eşleşmesi Izgara veya Ölü olmayan (Normal Canlı) balıklar
                    if (!isGrilled && !isDead)
                    {
                        if (IsFishCheckedInFilter(settings, rawName, "Öldür"))
                        {
                            killableMatches.Add(slotMatch);
                        }
                    }
                }
            }

            return killableMatches;
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
        /// Ayarlarda belirtilen balık adı için ilgili sütunun (Örn: "Öldür") seçili olup olmadığını kontrol eder.
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
