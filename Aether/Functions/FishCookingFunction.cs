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
    /// Envanter balık alanı dolduğunda (boş slot kalmadığında) "Pişir" seçeneği aktif olan balıkları
    /// kamp ateşi kurarak tek tek ateşe sürükleyip bırakan (Drag & Drop) modüler pişirme fonksiyon sınıfı.
    /// </summary>
    public static class FishCookingFunction
    {
        /// <summary>
        /// Pişirme sürecini baştan sona yönetir:
        /// 1. FishFilter tablosunda "Pişir" seçeneği aktif olan balıkların listesini çıkarır.
        /// 2. InventoryPosition bölgesindeki kamp ateşlerinden (ates.png) birine sağ tıklar.
        /// 3. FisherManSearchArea bölgesinde KampAtesiFloor ve KampAtesiFloor2 şablonlarını (>= %60) arar ve konumunu alır.
        /// 4. Pişir seçilmiş tüm balıkları tespit edip tek tek kamp ateşine sürükle-bırak (Drag & Drop) yapar.
        ///    Her bırakma anında kamp ateşinin varlığı teyit edilir.
        /// 5. Pişirme sonrası toplam pişirilen balık sayısını döner.
        /// </summary>
        public static async Task<bool> ExecuteCookingProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return false;

            // 1. ADIM: "Pişir" seçeneği aktif olan şablonları belirle
            var cookableTemplates = GetCookableTemplates(settings);
            if (cookableTemplates.Count == 0)
            {
                BotLogger.LogInfo(clientInfo.Id, "FishFilter ayarlarında 'Pişir' seçeneği aktif olan hiçbir balık/öğe bulunmuyor.");
                return false;
            }

            BotLogger.LogInfo(clientInfo.Id, $"🔥 Pişirme işlemi başlatılıyor. 'Pişir' işaretli {cookableTemplates.Count} şablon tespit edildi.");

            // 2. ADIM: Envanterde 'ates.png' ara
            TemplateMatchResult? fireMatch = FindCampfireInInventory(clientInfo.Handle);
            if (fireMatch == null || !fireMatch.IsSuccess)
            {
                BotLogger.LogWarning(clientInfo.Id, "⚠️ Envanterde kamp ateşi (ates.png) bulunamadı! Balıklar pişirilemiyor.");
                return false;
            }

            // Ateşin koordinatını hesapla ve sağ tıkla
            int fireLocalX = RegionConstants.InventoryPosition.StartX + fireMatch.Location.X + (fireMatch.Bounds.Width / 2);
            int fireLocalY = RegionConstants.InventoryPosition.StartY + fireMatch.Location.Y + (fireMatch.Bounds.Height / 2);

            BotLogger.LogInfo(clientInfo.Id, $"Kamp ateşine ({fireLocalX}, {fireLocalY}) sağ tıklanarak yere kuruluyor...");
            await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, fireLocalX, fireLocalY, fastMove: false, cancellationToken: cancellationToken);
            await Task.Delay(Random.Shared.Next(600, 900), cancellationToken);

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // 3. ADIM: FisherManSearchArea içinde KampAtesiFloor / KampAtesiFloor2 ara (>= %60)
            TemplateMatchResult? floorFireMatch = await WaitForFloorCampfireAsync(clientInfo.Handle, cancellationToken);
            if (floorFireMatch == null)
            {
                BotLogger.LogError(clientInfo.Id, "❌ Yerdeki Kamp Ateşi (KampAtesiFloor / KampAtesiFloor2) tespit edilemedi! Pişirme iptal ediliyor.");
                return false;
            }

            BotLogger.LogSuccess(clientInfo.Id, $"🔥 Yerdeki Kamp Ateşi tespit edildi! Konum: ({floorFireMatch.Location.X}, {floorFireMatch.Location.Y}), Benzerlik: %{floorFireMatch.Confidence * 100:F1}");

            // 4. ADIM: Pişir seçili tüm balıkları tek tek ateşe sürükle ve bırak
            int totalCooked = 0;
            const int maxIterations = 40; // Güvenlik döngü limiti
            int iteration = 0;

            while (iteration < maxIterations && !cancellationToken.IsCancellationRequested)
            {
                iteration++;

                // HER BIRAKMA / DÖNGÜ ÖNCESİ ATEŞİN VARLIĞINI TEYİT ET
                floorFireMatch = FindFloorCampfire(clientInfo.Handle);
                if (floorFireMatch == null)
                {
                    BotLogger.LogWarning(clientInfo.Id, "⚠️ Yerdeki kamp ateşi söndü veya kayboldu!");

                    // Envanterde başka ateş var mı kontrol et, varsa tekrar yak
                    var nextFire = FindCampfireInInventory(clientInfo.Handle);
                    if (nextFire != null && nextFire.IsSuccess)
                    {
                        int nfx = RegionConstants.InventoryPosition.StartX + nextFire.Location.X + (nextFire.Bounds.Width / 2);
                        int nfy = RegionConstants.InventoryPosition.StartY + nextFire.Location.Y + (nextFire.Bounds.Height / 2);
                        BotLogger.LogInfo(clientInfo.Id, "Envanterdeki diğer kamp ateşine sağ tıklanarak tekrar yakılıyor...");
                        await HumanMouseService.Instance.RightClickLocalAsync(clientInfo.Handle, nfx, nfy, fastMove: false, cancellationToken: cancellationToken);
                        await Task.Delay(Random.Shared.Next(600, 900), cancellationToken);
                        await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

                        floorFireMatch = await WaitForFloorCampfireAsync(clientInfo.Handle, cancellationToken);
                    }

                    if (floorFireMatch == null)
                    {
                        BotLogger.LogWarning(clientInfo.Id, "Yeni kamp ateşi kurulamadı. Pişirme döngüsü sonlandırılıyor.");
                        break;
                    }
                }

                // Envanter balık alanındaki pişirilebilir balıkları tara
                List<TemplateMatchResult> fishToCook = ScanFishToCook(clientInfo.Handle, cookableTemplates);
                if (fishToCook.Count == 0)
                {
                    BotLogger.LogSuccess(clientInfo.Id, $"✅ Pişirilecek başka balık kalmadı. Toplam {totalCooked} adet balık pişirildi.");
                    break;
                }

                var currentFish = fishToCook[0];
                int fishLocalX = RegionConstants.InventoryFishArea.StartX + currentFish.Location.X + (currentFish.Bounds.Width / 2);
                int fishLocalY = RegionConstants.InventoryFishArea.StartY + currentFish.Location.Y + (currentFish.Bounds.Height / 2);

                int fireTargetX = floorFireMatch.Location.X;
                int fireTargetY = floorFireMatch.Location.Y;

                BotLogger.LogInfo(clientInfo.Id, $"[{totalCooked + 1}] '{currentFish.TemplateName}' balığı ({fishLocalX}, {fishLocalY}) kamp ateşine ({fireTargetX}, {fireTargetY}) sürükleniyor...");

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
                await Task.Delay(Random.Shared.Next(350, 500), cancellationToken);

                // HER BIRAKMA ANINDA KampAtesiFloor ve KampAtesiFloor2 ŞABLONUNUN VARLIĞINI TEYİT ET
                var verifyFire = FindFloorCampfire(clientInfo.Handle);
                if (verifyFire == null)
                {
                    BotLogger.LogWarning(clientInfo.Id, "⚠️ Balık bırakıldıktan sonra kamp ateşi teyit edilemedi (ateş sönmüş olabilir).");
                }
                else
                {
                    BotLogger.LogInfo(clientInfo.Id, $"Kamp ateşi varlığı teyit edildi (Benzerlik: %{verifyFire.Confidence * 100:F1}).");
                }
            }

            // Pişirme işlemi bittiğinde fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(200, cancellationToken);

            return totalCooked > 0;
        }

        /// <summary>
        /// FishFilter ayarlarında "Pişir" seçeneği işaretli olan tüm şablon yollarını döndürür.
        /// </summary>
        public static List<string> GetCookableTemplates(FishBotSettings settings)
        {
            var cookable = new List<string>();
            if (settings == null || settings.FishFilter == null) return cookable;

            foreach (var templatePath in TemplateConstants.FishIconTemplates.All)
            {
                string itemName = Path.GetFileNameWithoutExtension(templatePath);

                // 1. Doğrudan sözlük kontrolü
                bool isCook = false;
                foreach (var category in settings.FishFilter.Values)
                {
                    if (category.TryGetValue(itemName, out var filterItem))
                    {
                        if (filterItem.GetCheck("Pişir", false))
                        {
                            isCook = true;
                            break;
                        }
                    }
                }

                // 2. Normalizasyon ile eşleştirme (Fallback)
                if (!isCook)
                {
                    string normItem = NormalizeKey(itemName);
                    foreach (var category in settings.FishFilter.Values)
                    {
                        foreach (var kvp in category)
                        {
                            if (NormalizeKey(kvp.Key) == normItem)
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
        /// InventoryFishArea bölgesinde pişirilmesi gereken balıkları tarar ve çakışmaları ayıklayarak döndürür.
        /// </summary>
        private static List<TemplateMatchResult> ScanFishToCook(IntPtr hWnd, List<string> cookableTemplates)
        {
            var matches = new List<TemplateMatchResult>();

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return matches;

                // Canlı ve ölü balıkların renk farkını koruyarak sadece doğru balıkları pişirmek için useGrayscale: false kullanılır
                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, cookableTemplates, threshold: 0.80, useGrayscale: false);

                // Çakışan mükerrer tespitleri ayıkla
                foreach (var m in allFound)
                {
                    if (!matches.Any(existing => Math.Abs(existing.Location.X - m.Location.X) < 14 && Math.Abs(existing.Location.Y - m.Location.Y) < 14))
                    {
                        matches.Add(m);
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// InventoryPosition bölgesinde 'ates.png' şablonunu arar.
        /// </summary>
        private static TemplateMatchResult? FindCampfireInInventory(IntPtr hWnd)
        {
            using (Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryPosition))
            {
                if (invBmp == null) return null;

                var matches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.70);
                if (matches.Count > 0)
                {
                    return matches[0];
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
        private static async Task<TemplateMatchResult?> WaitForFloorCampfireAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            const int maxAttempts = 10;
            const int delayBetweenAttemptsMs = 250;

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
