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
    /// Envanterdeki "Yere At" seçeneği aktif olan balıkları, diğer öğeleri ve materyalleri
    /// envanter dışına sürükleyip bırakarak ve onay penceresine tıklayarak yere atan modüler fonksiyon sınıfı.
    ///
    /// ALGORİTMA:
    /// A: Tüm slotlarda fareyi gezdir. (HoverAcrossInventoryFishAreaAsync)
    /// B: "Yere At" seçeneği olan öğeler var mı diye kontrol et.
    ///    - GrilledFishes, DeadFishes, FishIconTemplates, Others, DeadFishLoot şablonları ile matching yap.
    ///    - Eğer en yüksek eşleşen şablon için kullanıcının ayarlarında "Yere At" seçiliyse listeye ekle.
    /// C1: Yere atılabilir öğeleri sırayla tut ve InventoryPosition regionunun solunda (100px mesafede rastgele bir alana) bırak.
    /// C2: Her bıraktığında 50ms sonra DropItemQuestionArea içerisinde DropItemQuestion ve DropItemQuestionYesButton ara.
    /// C3: DropItemQuestionYesButton pozisyonuna tıkla.
    /// D: Yere atılacak öğe kalmadığında fareyi envanter dışına çek ve pişirme adımına devam et.
    /// </summary>
    public static class FishDropFunction
    {
        /// <summary>
        /// Yere atma sürecini baştan sona yönetir.
        /// </summary>
        public static async Task<bool> ExecuteDropProcessAsync(ClientInfo clientInfo, FishBotSettings settings, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested)
                return false;

            // =========================================================================
            // ADIM A: Tüm slotlarda fareyi gezdir
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Yere Atma - Adım A] InventoryFishArea slotları üzerinde fare gezdiriliyor...");
            await FishingExecutionFunction.HoverAcrossInventoryFishAreaAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(Random.Shared.Next(100, 200), cancellationToken);

            // =========================================================================
            // ADIM B: "Yere At" seçeneği olan öğeler var mı diye kontrol et
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, "[Yere Atma - Adım B] Yere atılacak öğeler taranıyor...");
            var droppableItems = ScanDroppableItemsInFishArea(clientInfo.Handle, settings);

            if (droppableItems.Count == 0)
            {
                BotLogger.LogInfo(clientInfo.Id, "[Yere Atma] Envanterde 'Yere At' seçeneği aktif öğe bulunamadı.");
                return true;
            }

            BotLogger.LogInfo(clientInfo.Id, $"🗑️ [Yere Atma] Envanterde {droppableItems.Count} adet yere atılacak öğe tespit edildi.");

            // =========================================================================
            // ADIM C1 - C3: Sırayla yere at
            // =========================================================================
            int droppedCount = 0;
            for (int i = 0; i < droppableItems.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var item = droppableItems[i];
                int itemLocalX = RegionConstants.InventoryFishArea.StartX + item.Location.X + (item.Bounds.Width / 2);
                int itemLocalY = RegionConstants.InventoryFishArea.StartY + item.Location.Y + (item.Bounds.Height / 2);

                // InventoryPosition regionunun dışarısında - solunda 100px'e kadarlık rastgele bir alan
                int dropLocalX = RegionConstants.InventoryPosition.StartX - Random.Shared.Next(30, 95);
                int dropLocalY = RegionConstants.InventoryPosition.StartY + Random.Shared.Next(40, RegionConstants.InventoryPosition.Height - 40);

                BotLogger.LogInfo(clientInfo.Id, $"[{i + 1}/{droppableItems.Count}] '{item.TemplateName}' ({itemLocalX}, {itemLocalY}) yere bırakılıyor ({dropLocalX}, {dropLocalY})...");

                // Sürükle ve bırak (Drag & Drop)
                await HumanMouseService.Instance.DragAndDropLocalAsync(
                    clientInfo.Handle,
                    itemLocalX,
                    itemLocalY,
                    dropLocalX,
                    dropLocalY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                // C2: 50ms bekle ve DropItemQuestionArea içerisinde soru ve Yes butonunu ara
                await Task.Delay(50, cancellationToken);

                bool confirmed = await HandleDropConfirmationAsync(clientInfo.Handle, cancellationToken);
                if (confirmed)
                {
                    BotLogger.LogInfo(clientInfo.Id, $"[{i + 1}/{droppableItems.Count}] '{item.TemplateName}' onaylanarak yere atıldı.");
                }

                droppedCount++;
                await Task.Delay(Random.Shared.Next(150, 250), cancellationToken);
            }

            // Fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(100, cancellationToken);

            BotLogger.LogSuccess(clientInfo.Id, $"✅ [Yere Atma - Adım D] Toplam {droppedCount} adet öğe yere atıldı. Pişirme adımına devam ediliyor.");
            return true;
        }

        /// <summary>
        /// DropItemQuestionArea içinde 'DropItemQuestion' ve 'DropItemQuestionYesButton' şablonlarını arar ve tıklar.
        /// Tıkladıktan sonra 50ms bekleyip DropItemQuestion şablonunu tekrar kontrol eden bir döngüye girer.
        /// Soru penceresi hala açıksa tekrar 'Evet' butonuna tıklar, pencere kapandığında döngüden çıkar.
        /// </summary>
        private static async Task<bool> HandleDropConfirmationAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            // 1. Pop-up'ın ekranda belirmesini bekle (maks 5 deneme, 40ms aralıkla)
            bool popupFound = false;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (Bitmap? questionBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.DropItemQuestionArea))
                {
                    if (questionBmp != null)
                    {
                        var qMatch = TemplateConstants.Match(questionBmp, TemplateConstants.WindowParts.DropItemQuestion, threshold: 0.70);
                        var yesMatch = TemplateConstants.Match(questionBmp, TemplateConstants.WindowParts.DropItemQuestionYesButton, threshold: 0.70);

                        if (qMatch.IsSuccess || yesMatch.IsSuccess)
                        {
                            popupFound = true;
                            break;
                        }
                    }
                }

                await Task.Delay(40, cancellationToken);
            }

            if (!popupFound)
            {
                return false;
            }

            // 2. Evet butonuna tıkla ve 50ms sonra kapanıp kapanmadığını teyit et (Evete basıldığından emin olma döngüsü)
            for (int loop = 0; loop < 10; loop++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                using (Bitmap? checkBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.DropItemQuestionArea))
                {
                    if (checkBmp == null) break;

                    var qMatch = TemplateConstants.Match(checkBmp, TemplateConstants.WindowParts.DropItemQuestion, threshold: 0.70);
                    var yesMatch = TemplateConstants.Match(checkBmp, TemplateConstants.WindowParts.DropItemQuestionYesButton, threshold: 0.70);

                    // Soru penceresi veya evet butonu yoksa evete basılmış ve pencere başarıyla kapanmış demektir
                    if (!qMatch.IsSuccess && !yesMatch.IsSuccess)
                    {
                        return true;
                    }

                    // Soru penceresi hala açıksa Yes butonuna tıkla
                    if (yesMatch.IsSuccess)
                    {
                        int yesLocalX = RegionConstants.DropItemQuestionArea.StartX + yesMatch.Location.X + (yesMatch.Bounds.Width / 2);
                        int yesLocalY = RegionConstants.DropItemQuestionArea.StartY + yesMatch.Location.Y + (yesMatch.Bounds.Height / 2);

                        await HumanMouseService.Instance.LeftClickLocalAsync(hWnd, yesLocalX, yesLocalY, fastMove: true, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // Soru penceresi var ama Yes butonu tek başına eşleşmediyse soru kutusu içi varsayılan Evet buton koordinatına tıkla
                        int defaultYesX = RegionConstants.DropItemQuestionArea.StartX + (RegionConstants.DropItemQuestionArea.Width * 3 / 8);
                        int defaultYesY = RegionConstants.DropItemQuestionArea.StartY + (RegionConstants.DropItemQuestionArea.Height * 3 / 4);
                        await HumanMouseService.Instance.LeftClickLocalAsync(hWnd, defaultYesX, defaultYesY, fastMove: true, cancellationToken: cancellationToken);
                    }
                }

                // Tıkladıktan sonra 50ms bekle ve döngü başında DropItemQuestion kontrolünü tekrar yap
                await Task.Delay(50, cancellationToken);
            }

            return true;
        }

        /// <summary>
        /// InventoryFishArea bölgesinde TÜM balık, nesne ve ganimet şablonlarını tarar.
        /// Slot bazında en yüksek benzerliğe sahip şablonu belirler (NMS).
        /// Kullanıcının ayarlarında "Yere At" seçeneği işaretli olanları döndürür.
        /// </summary>
        public static List<TemplateMatchResult> ScanDroppableItemsInFishArea(IntPtr hWnd, FishBotSettings settings)
        {
            var droppableMatches = new List<TemplateMatchResult>();
            if (settings == null || settings.FishFilter == null) return droppableMatches;

            var allTemplates = GetAllDroppableTemplates();
            if (allTemplates.Count == 0) return droppableMatches;

            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(hWnd, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null) return droppableMatches;

                var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, allTemplates, threshold: 0.80, useGrayscale: false);
                if (allFound == null || allFound.Count == 0) return droppableMatches;

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

                foreach (var slotMatch in bestSlotMatches)
                {
                    string rawName = Path.GetFileNameWithoutExtension(slotMatch.TemplatePath);

                    bool isGrilled = rawName.StartsWith("Izgara_", StringComparison.OrdinalIgnoreCase) ||
                                     slotMatch.TemplatePath.Contains("Izgara_", StringComparison.OrdinalIgnoreCase);

                    // Izgaralar KESİNLİKLE yere atılmaz! Yalnızca Canlı balıklar, Ölü balıklar ve Diğer öğeler/ganimetler atılabilir.
                    if (isGrilled) continue;

                    if (IsItemCheckedInFilter(settings, rawName, "Yere At"))
                    {
                        droppableMatches.Add(slotMatch);
                    }
                }
            }

            return droppableMatches;
        }

        /// <summary>
        /// Yere atma kontrolü için taranacak TÜM şablon listesini (Canlı, Ölü, Izgara, Diğer Nesneler, Ganimetler) döndürür.
        /// </summary>
        public static List<string> GetAllDroppableTemplates()
        {
            var list = new List<string>();
            list.AddRange(TemplateConstants.FishIconTemplates.Common.All);
            list.AddRange(TemplateConstants.FishIconTemplates.Rare.All);
            list.AddRange(TemplateConstants.FishIconTemplates.DeadFishes.All);
            list.AddRange(TemplateConstants.FishIconTemplates.GrilledFishes.All);
            list.AddRange(TemplateConstants.FishIconTemplates.Others.All);
            return list;
        }

        /// <summary>
        /// Ayarlarda belirtilen öğe adı için ilgili sütunun (Örn: "Yere At") seçili olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsItemCheckedInFilter(FishBotSettings settings, string itemName, string checkColumnName)
        {
            if (settings == null || settings.FishFilter == null) return false;

            string baseItemName = itemName.Replace("Ölü_", "").Replace("Izgara_", "").Trim();

            foreach (var category in settings.FishFilter.Values)
            {
                if (category.TryGetValue(itemName, out var filterItem) || category.TryGetValue(baseItemName, out filterItem))
                {
                    if (filterItem.GetCheck(checkColumnName, false))
                    {
                        return true;
                    }
                }
            }

            string normBase = NormalizeKey(baseItemName);
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
