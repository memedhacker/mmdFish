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
    /// A: Tüm slotlarda fareyi gezdir. (HoverAcrossInventoryFishAreaAsync)
    /// B: "Öldür" seçeneği olan balıklar var mı diye kontrol et.
    ///    - GrilledFishes, DeadFishes, FishIconTemplates (Common, Rare) şablonları ile matching işlemi yap.
    ///    - Eğer aynı pozisyonda en yüksek eşleşme FishIconTemplates (ne Izgara_ ne de Ölü_ ile başlayan) ise bu balık öldürülebilir demektir.
    ///    - Ve kullanıcının ayarlarında bu balık için "Öldür" sütunu seçiliyse listeye ekle.
    /// C: Öldürülebilir balıklara sırayla birer kez sağ tıkla.
    /// </summary>
    public static class FishKillingFunction
    {
        /// <summary>
        /// Öldürme sürecini baştan sona yönetir.
        /// </summary>
        public static async Task<bool> ExecuteKillingProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return false;

            // =========================================================================
            // ADIM A: Tüm slotlarda fareyi gezdir
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Balık Öldürme - Adım A] InventoryFishArea slotları üzerinde fare gezdiriliyor...");
            await FishingExecutionFunction.HoverAcrossInventoryFishAreaAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(Random.Shared.Next(100, 200), cancellationToken);

            // =========================================================================
            // ADIM B: "Öldür" seçeneği olan balıklar var mı diye kontrol et
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Balık Öldürme - Adım B] Öldürülebilir canlı balıklar taranıyor...");
            var killableFish = ScanKillableFishInFishArea(clientInfo.Handle, settings);

            if (killableFish.Count == 0)
            {
                BotLogger.LogInfo(clientInfo.Id, "[Balık Öldürme] Envanterde 'Öldür' seçeneği aktif canlı balık bulunamadı.");
                return false;
            }

            BotLogger.LogInfo(clientInfo.Id, $"⚔️ [Balık Öldürme] Envanterde {killableFish.Count} adet öldürülecek canlı balık tespit edildi.");

            // =========================================================================
            // ADIM C: Öldürülebilir balıklara sırayla birer kez sağ tıkla
            // =========================================================================
            int killCount = 0;
            foreach (var fish in killableFish)
            {
                if (cancellationToken.IsCancellationRequested) break;

                int fishLocalX = RegionConstants.InventoryFishArea.StartX + fish.Location.X + (fish.Bounds.Width / 2);
                int fishLocalY = RegionConstants.InventoryFishArea.StartY + fish.Location.Y + (fish.Bounds.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"[{killCount + 1}/{killableFish.Count}] '{fish.TemplateName}' ({fishLocalX}, {fishLocalY}) öldürmek için sağ tıklanıyor...");
                await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, fishLocalX, fishLocalY, fastMove: false, cancellationToken: cancellationToken);
                killCount++;

                await Task.Delay(Random.Shared.Next(200, 350), cancellationToken);
            }

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(100, cancellationToken);

            BotLogger.LogSuccess(clientInfo.Id, $"✅ [Balık Öldürme] Toplam {killCount} adet balık öldürüldü.");
            return killCount > 0;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde TÜM balık şablonlarını (GrilledFishes, DeadFishes, FishIconTemplates) tarar.
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

                // TÜM balık şablonları ile renk korumalı arama (threshold: 0.80)
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

                // 3. Filtreleme: Yalnızca en iyi eşleşmesi Izgara_ veya Ölü_ OLMAYAN (FishIconTemplates - Canlı) ve "Öldür" işaretli olanları seç
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
