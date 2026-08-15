using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using Aether.States;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Balıkçı NPC arama, kamera açısını orta tuşla sağa döndürme ve marketi açma modülü.
    /// Tüm adımları BotLogger ile logPanel üzerine yazdırır.
    /// </summary>
    public static class StartupFishermanFunction
    {
        /// <summary>
        /// A - G adımlarını sırasıyla yürütür:
        /// A: InventoryFishArea'da 4. slot ve sonrasında boş yer var mı kontrol et. [EmptySlot template]
        /// B: Boş yer varsa Balikci ve Balikci2 yi ara. [250ms ile]
        /// C: Balıkçı bulunamazsa fareyi ekranın ortasına getir ve fare tekerleğine basılı tut.
        /// D: Fare tekerleği basılı tutuluyken 100px kadar sağa kaydır ve basılı tutmayı bırak.
        /// E: "B" Adımına geri dön ve tekrar balıkçıyı ara.
        /// F: Eğer balıkçı bulunduysa üzerine tıkla ve 1 saniye bekle.
        /// G: OpenMarketPosition Regionuna git ve tıkla.
        /// </summary>
        public static async Task ExecuteAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);
            if (!settings.BuyWormEnabled)
            {
                BotLogger.LogInfo(clientInfo.Id, "BuyWorm pasif olduğu için balıkçı arama adımı atlandı.");
                return;
            }

            BotLogger.LogInfo(clientInfo.Id, "[Adım A] InventoryFishArea (4. slot ve sonrası) boş yer kontrolü yapılıyor...");

            // Tarama öncesi fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // =========================================================================
            // ADIM A: InventoryFishArea'da 4. slot ve sonrasında boş yer var mı kontrol et
            // =========================================================================
            using (Bitmap? fishAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryFishArea))
            {
                if (fishAreaBmp == null)
                {
                    BotLogger.LogError(clientInfo.Id, "[Adım A] InventoryFishArea ekran görüntüsü alınamadı.");
                    return;
                }

                var emptyMatches = TemplateConstants.MatchAll(fishAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);

                // 4. slot ve sonrasındaki boş yerleri filtrele (1. satırın ilk 3 slotu hariç)
                var emptySlotsInSlot4Plus = emptyMatches
                    .Where(e => !IsFirstThreeSlotsOfFishArea(e.Location.X + (e.Bounds.Width / 2), e.Location.Y + (e.Bounds.Height / 2)))
                    .ToList();

                if (emptySlotsInSlot4Plus.Count == 0)
                {
                    BotLogger.LogInfo(clientInfo.Id, "[Adım A] InventoryFishArea 4. slot ve sonrasında boş yer yok. Balıkçı aranmayacak.");
                    return;
                }

                BotLogger.LogInfo(clientInfo.Id, $"[Adım A] 4. slot ve sonrasında {emptySlotsInSlot4Plus.Count} adet boş yer tespit edildi.");
            }

            // =========================================================================
            // ADIM B - E: Balıkçı Arama ve Kamerayı Sağa Çevirme Döngüsü
            // =========================================================================
            TemplateMatchResult? foundFisherman = null;
            const int maxRotationAttempts = 20;

            for (int attempt = 0; attempt <= maxRotationAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                if (attempt > 0)
                {
                    // ADIM C & D: Fareyi ekranın ortasına getir, orta tuşa (tekerlek) basılı tut, 20px sağa kaydır ve bırak
                    BotLogger.LogWarning(clientInfo.Id, $"[Adım C-D] Balıkçı bulunamadı. Fare merkezde orta tuşla 20px sağa kaydırılıyor ({attempt}/{maxRotationAttempts})...");
                    await RotateCameraRight20PxAsync(clientInfo.Handle, cancellationToken);
                    await Task.Delay(350, cancellationToken);
                }

                // ADIM B: Balikci ve Balikci2 yi ara [250ms arayla]
                BotLogger.LogInfo(clientInfo.Id, $"[Adım B] Balıkçı NPC aranıyor (Döndürme #{attempt})...");
                foundFisherman = await SearchFishermanAsync(clientInfo, cancellationToken);

                if (foundFisherman != null && foundFisherman.IsSuccess)
                {
                    BotLogger.LogSuccess(clientInfo.Id, $"[Adım B] Balıkçı başarıyla bulundu! (X: {foundFisherman.CenterPoint.X}, Y: {foundFisherman.CenterPoint.Y}, %{foundFisherman.Confidence * 100:F1})");
                    break;
                }
            }

            // KRİTİK KONTROL: Eğer balıkçı bulunamadıysa ASLA devam etme!
            if (foundFisherman == null || !foundFisherman.IsSuccess)
            {
                BotLogger.LogError(clientInfo.Id, "❌ Balıkçı NPC tüm kamera çevrimlerine rağmen bulunamadı! İşlem durduruluyor, OpenMarketPosition tıklanmayacak.");
                return;
            }

            // =========================================================================
            // ADIM F: Eğer balıkçı bulunduysa üzerine tıkla ve 1 saniye bekle
            // =========================================================================
            BotLogger.LogInfo(clientInfo.Id, $"[Adım F] Balıkçı üzerine tıklanıyor ({foundFisherman.CenterPoint.X}, {foundFisherman.CenterPoint.Y}) ve 1 sn bekleniyor...");
            await HumanMouseService.Instance.LeftClickLocalAsync(
                clientInfo.Handle,
                foundFisherman.CenterPoint.X,
                foundFisherman.CenterPoint.Y,
                fastMove: false,
                cancellationToken: cancellationToken);

            await Task.Delay(1000, cancellationToken);

            // =========================================================================
            // ADIM G: OpenMarketPosition Regionuna git ve tıkla
            // =========================================================================
            int openMarketCenterX = RegionConstants.OpenMarketPosition.StartX + (RegionConstants.OpenMarketPosition.Width / 2);
            int openMarketCenterY = RegionConstants.OpenMarketPosition.StartY + (RegionConstants.OpenMarketPosition.Height / 2);

            BotLogger.LogSuccess(clientInfo.Id, $"[Adım G] OpenMarketPosition merkezine ({openMarketCenterX}, {openMarketCenterY}) gidilip tıklanıyor...");
            await HumanMouseService.Instance.LeftClickLocalAsync(
                clientInfo.Handle,
                openMarketCenterX,
                openMarketCenterY,
                fastMove: false,
                cancellationToken: cancellationToken);

            BotLogger.LogSuccess(clientInfo.Id, "[Adım G] OpenMarketPosition başarıyla tıklandı.");
        }

        /// <summary>
        /// ADIM B: FisherManSearchArea bölgesinde Balikci ve Balikci2 şablonlarını 250ms arayla arar (Eşik: %58).
        /// </summary>
        private static async Task<TemplateMatchResult?> SearchFishermanAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            const int maxChecks = 8;
            const int intervalMs = 250;
            const double threshold = 0.58; // Yanlış eşleşmeleri engellemek için %58 eşik

            var searchArea = RegionConstants.FisherManSearchArea;

            for (int i = 1; i <= maxChecks; i++)
            {
                if (cancellationToken.IsCancellationRequested) return null;

                using (Bitmap? areaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, searchArea))
                {
                    if (areaBmp != null)
                    {
                        // 1. Önce Balikci.png şablonunu dene
                        var match1 = TemplateConstants.Match(areaBmp, TemplateConstants.Fisherman.Balikci, threshold: threshold);
                        if (match1.IsSuccess && match1.Confidence >= threshold)
                        {
                            return ToWindowCoordinates(match1, searchArea);
                        }

                        // 2. Ardından Balikci2.png şablonunu dene
                        var match2 = TemplateConstants.Match(areaBmp, TemplateConstants.Fisherman.Balikci2, threshold: threshold);
                        if (match2.IsSuccess && match2.Confidence >= threshold)
                        {
                            return ToWindowCoordinates(match2, searchArea);
                        }
                    }
                }

                if (i < maxChecks)
                    await Task.Delay(intervalMs, cancellationToken);
            }

            return null;
        }

        private static TemplateMatchResult ToWindowCoordinates(TemplateMatchResult match, WindowRegion region)
        {
            int absX = match.Location.X + region.StartX;
            int absY = match.Location.Y + region.StartY;
            match.Location = new System.Drawing.Point(absX, absY);
            match.Bounds = new Rectangle(absX, absY, match.Bounds.Width, match.Bounds.Height);
            return match;
        }

        /// <summary>
        /// ADIM C & D: Fareyi ekranın tam ortasına getirir, fare tekerleğine (orta tuş) basılı tutar,
        /// 20px sağa kaydırır ve basılı tutmayı bırakır.
        /// </summary>
        private static async Task RotateCameraRight20PxAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd)) return;

            Win32Native.SetForegroundWindow(hWnd);
            await Task.Delay(50, cancellationToken);

            int centerX = 400;
            int centerY = 300;

            if (Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect) && clientRect.Width > 0 && clientRect.Height > 0)
            {
                centerX = clientRect.Width / 2;
                centerY = clientRect.Height / 2;
            }

            int targetX = centerX + 20;
            int targetY = centerY;

            Debug.WriteLine($"[StartupFisherman] Kamera 20px sağa kaydırma: ({centerX}, {centerY}) -> ({targetX}, {targetY}) [MouseWheel Drag]");
            await HumanMouseService.Instance.MiddleDragAndDropLocalAsync(
                hWnd,
                centerX,
                centerY,
                targetX,
                targetY,
                fastMove: false,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Koordinatın InventoryFishArea'nın ilk 3 slotuna (1. satır, ilk 3 sütun) ait olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsFirstThreeSlotsOfFishArea(int localXInFishArea, int localYInFishArea)
        {
            int columnWidth = RegionConstants.InventoryFishArea.Width / 5; // ~34 px
            int rowHeight = RegionConstants.InventoryFishArea.Height / 7;  // ~33 px

            // 1. Satırda mı?
            bool isFirstRow = localYInFishArea < (rowHeight + 5);

            // İlk 3 sütunda mı? (Sütun 1, 2, 3)
            bool isFirstThreeCols = localXInFishArea < (columnWidth * 3 + 5);

            return isFirstRow && isFirstThreeCols;
        }
    }
}
