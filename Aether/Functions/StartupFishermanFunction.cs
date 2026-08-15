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
    /// Balıkçı NPC arama, kamera açısını orta tuşla sağa döndürme, marketi açma,
    /// doğrulama (MarketiAc >= %60, MarketTitle >= %90), yem ve kamp ateşi satın alma ve tam envanter organizasyonu modülü.
    /// </summary>
    public static class StartupFishermanFunction
    {
        /// <summary>
        /// A - G adımlarını ve market satın alma/düzenleme döngüsünü sırasıyla yürütür:
        /// 1. InventoryBaitArea boşluk (yem ve ateş) kontrolü
        /// 2. Balıkçı arama & 20px sağa kamera çevirme döngüsü
        /// 3. Balıkçıya tıklama & 1 sn bekleme
        /// 4. OpenMarketPosition bölgesinde MarketiAc (%60 üzeri) doğrulama
        /// 5. OpenMarketPosition tıklama & 1 sn bekleme
        /// 6. FisherManSearchArea bölgesinde MarketTitle (%90 üzeri) doğrulama
        /// 7. Yem satın alma, stackleme ve 4.+ slotlara düzenleme döngüsü
        /// 8. Kamp ateşi kontrolü (ilk 3 slot), boşluk x 2 kadar satın alma ve ilk 3 slota yerleştirme
        /// 9. MarketExitButtonPosition tıklayarak marketi kapatma
        /// </summary>
        public static async Task ExecuteAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);
            if (!settings.BuyWormEnabled && !settings.BuyCampfireEnabled)
            {
                BotLogger.LogInfo(clientInfo.Id, "BuyWorm ve BuyCampfire pasif olduğu için balıkçı arama adımı atlandı.");
                return;
            }

            BotLogger.LogInfo(clientInfo.Id, "[Adım A] InventoryBaitArea boş yer kontrolü yapılıyor...");

            // Tarama öncesi fareyi envanter dışına çek
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            bool needWorms = false;
            bool needFire = false;

            // =========================================================================
            // ADIM A: InventoryBaitArea'da boş yer kontrolü (Yem ve Ateş)
            // =========================================================================
            using (Bitmap? baitAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryBaitArea))
            {
                if (baitAreaBmp == null)
                {
                    BotLogger.LogError(clientInfo.Id, "[Adım A] InventoryBaitArea ekran görüntüsü alınamadı.");
                    return;
                }

                if (settings.BuyWormEnabled)
                {
                    var emptyMatches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);
                    var baitMatches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60);

                    var emptySlotsInSlot4Plus = emptyMatches
                        .Where(e => !IsFirstThreeSlotsOfBaitAreaDirect(e.Location.X + (e.Bounds.Width / 2), e.Location.Y + (e.Bounds.Height / 2)))
                        .Where(e => !baitMatches.Any(b => Math.Abs(b.Location.X - e.Location.X) < 16 && Math.Abs(b.Location.Y - e.Location.Y) < 16))
                        .ToList();

                    needWorms = emptySlotsInSlot4Plus.Count > 0;
                    BotLogger.LogInfo(clientInfo.Id, $"[Adım A] 4.+ slotlarda {emptySlotsInSlot4Plus.Count} adet boş yem slotu tespit edildi.");
                }

                if (settings.BuyCampfireEnabled)
                {
                    int emptyFireSlots = CountEmptyFirstThreeSlots(baitAreaBmp);
                    needFire = emptyFireSlots > 0;
                    BotLogger.LogInfo(clientInfo.Id, $"[Adım A] İlk 3 slotta {emptyFireSlots} adet boş ateş slotu tespit edildi.");
                }

                if (!needWorms && !needFire)
                {
                    BotLogger.LogInfo(clientInfo.Id, "[Adım A] Yem ve ateş slotları tamamen dolu. Balıkçı aranmayacak.");
                    return;
                }
            }

            // =========================================================================
            // BALIKÇI ARAMA, TIKLAMA VE MARKETİ AÇMA DÖNGÜSÜ (DOĞRULAMALAR İLE)
            // =========================================================================
            const int maxMarketOpenAttempts = 5;
            bool marketOpenedSuccessfully = false;

            for (int marketAttempt = 1; marketAttempt <= maxMarketOpenAttempts; marketAttempt++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                if (marketAttempt > 1)
                {
                    BotLogger.LogWarning(clientInfo.Id, $"Market açma denemesi #{marketAttempt}/{maxMarketOpenAttempts} başlatılıyor...");
                }

                TemplateMatchResult? foundFisherman = null;
                const int maxRotationAttempts = 20;

                // ADIM B - E: Balıkçı Arama ve Kamerayı Sağa Çevirme Döngüsü
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

                // ADIM F: Balıkçı üzerine tıkla ve 1 saniye bekle
                BotLogger.LogInfo(clientInfo.Id, $"[Adım F] Balıkçı üzerine tıklanıyor ({foundFisherman.CenterPoint.X}, {foundFisherman.CenterPoint.Y}) ve 1 sn bekleniyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(
                    clientInfo.Handle,
                    foundFisherman.CenterPoint.X,
                    foundFisherman.CenterPoint.Y,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                await Task.Delay(1000, cancellationToken);

                // =========================================================================
                // KONTROL 1: OpenMarketPosition bölgesinde MarketiAc template'i kontrolü (>= %60)
                // =========================================================================
                using (Bitmap? openMarketBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.OpenMarketPosition))
                {
                    if (openMarketBmp == null)
                    {
                        BotLogger.LogWarning(clientInfo.Id, "OpenMarketPosition ekran görüntüsü alınamadı. Balıkçı arama adımına geri dönülüyor...");
                        continue;
                    }

                    var marketiAcMatch = TemplateConstants.Match(openMarketBmp, TemplateConstants.Fisherman.MarketiAc, threshold: 0.60);
                    if (!marketiAcMatch.IsSuccess || marketiAcMatch.Confidence < 0.60)
                    {
                        BotLogger.LogWarning(clientInfo.Id, $"OpenMarketPosition bölgesinde 'MarketiAc' butonu bulunamadı (%{marketiAcMatch.Confidence * 100:F1} < %60.0). Balıkçı arama adımına geri dönülüyor...");
                        continue;
                    }

                    BotLogger.LogSuccess(clientInfo.Id, $"[Marketi Aç Butonu] 'MarketiAc' başarıyla doğrulandı (%{marketiAcMatch.Confidence * 100:F1} >= %60.0).");
                }

                // ADIM G: OpenMarketPosition Regionuna git ve tıkla
                int openMarketCenterX = RegionConstants.OpenMarketPosition.StartX + (RegionConstants.OpenMarketPosition.Width / 2);
                int openMarketCenterY = RegionConstants.OpenMarketPosition.StartY + (RegionConstants.OpenMarketPosition.Height / 2);

                BotLogger.LogSuccess(clientInfo.Id, $"[Adım G] OpenMarketPosition merkezine ({openMarketCenterX}, {openMarketCenterY}) gidilip tıklanıyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(
                    clientInfo.Handle,
                    openMarketCenterX,
                    openMarketCenterY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "[Adım G] OpenMarketPosition tıklandı. Marketin açılması için 1 sn bekleniyor...");
                await Task.Delay(1000, cancellationToken);

                // =========================================================================
                // KONTROL 2: FisherManSearchArea içerisinde MarketTitle template'i kontrolü (>= %90)
                // =========================================================================
                using (Bitmap? searchAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.FisherManSearchArea))
                {
                    if (searchAreaBmp == null)
                    {
                        BotLogger.LogWarning(clientInfo.Id, "FisherManSearchArea ekran görüntüsü alınamadı. Balıkçı arama adımına geri dönülüyor...");
                        continue;
                    }

                    var marketTitleMatch = TemplateConstants.Match(searchAreaBmp, TemplateConstants.Fisherman.MarketTitle, threshold: 0.90);
                    if (!marketTitleMatch.IsSuccess || marketTitleMatch.Confidence < 0.90)
                    {
                        BotLogger.LogWarning(clientInfo.Id, $"FisherManSearchArea içerisinde 'MarketTitle' bulunamadı (%{marketTitleMatch.Confidence * 100:F1} < %90.0). Market açılmamış, balıkçı arama adımına geri dönülüyor...");
                        continue;
                    }

                    BotLogger.LogSuccess(clientInfo.Id, $"✅ Market başarıyla açıldı! (MarketTitle %{marketTitleMatch.Confidence * 100:F1} >= %90.0). Satın alma işlemlerine geçiliyor...");
                    marketOpenedSuccessfully = true;
                    break;
                }
            }

            if (!marketOpenedSuccessfully)
            {
                BotLogger.LogError(clientInfo.Id, "❌ Market tüm denemelere rağmen açılamadı! İşlem sonlandırılıyor.");
                return;
            }

            // =========================================================================
            // 1. BÖLÜM: YEM SATIN ALMA VE DÜZENLEME DÖNGÜSÜ (A -> A2 -> B -> C -> D)
            // =========================================================================
            if (settings.BuyWormEnabled)
            {
                int marketBaitCenterX = RegionConstants.MarketBaitPosition.StartX + (RegionConstants.MarketBaitPosition.Width / 2);
                int marketBaitCenterY = RegionConstants.MarketBaitPosition.StartY + (RegionConstants.MarketBaitPosition.Height / 2);

                int cycle = 0;
                const int maxCycles = 15;

                while (cycle < maxCycles && !cancellationToken.IsCancellationRequested)
                {
                    cycle++;

                    // A: InventoryBaitArea içerisinde ilk üç slot hariç tüm slotlar kontrol edilecek.
                    await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

                    int emptyCount = 0;
                    using (Bitmap? baitAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryBaitArea))
                    {
                        if (baitAreaBmp != null)
                        {
                            var emptyMatches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);
                            var baitMatches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60);

                            var emptySlots = emptyMatches
                                .Where(e => !IsFirstThreeSlotsOfBaitAreaDirect(e.Location.X + (e.Bounds.Width / 2), e.Location.Y + (e.Bounds.Height / 2)))
                                .Where(e => !baitMatches.Any(b => Math.Abs(b.Location.X - e.Location.X) < 16 && Math.Abs(b.Location.Y - e.Location.Y) < 16))
                                .ToList();

                            emptyCount = emptySlots.Count;
                        }
                    }

                    BotLogger.LogInfo(clientInfo.Id, $"[Market Döngüsü #{cycle}] InventoryBaitArea 4.+ slot kontrol edildi. Boş slot sayısı: {emptyCount}");

                    // A2: EĞER BOŞ SLOT YOKSA DÖNGÜDEN ÇIK.
                    if (emptyCount <= 0)
                    {
                        BotLogger.LogSuccess(clientInfo.Id, "✅ Envanterde yem için boş yer kalmadı. Yem satın alma döngüsü tamamlandı.");
                        break;
                    }

                    // B: Eğer boş slot varsa MarketBaitPosition ortasına mouse götürülüp 2 saniye aralıkla boş slot sayısı kadar tıklanacak.
                    BotLogger.LogInfo(clientInfo.Id, $"[Market Döngüsü #{cycle}] Marketteki yeme ({marketBaitCenterX}, {marketBaitCenterY}) 2 sn aralıkla {emptyCount} defa sağ tıklanıyor...");
                    for (int buyIdx = 1; buyIdx <= emptyCount; buyIdx++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        await HumanMouseService.Instance.RightClickLocalAsync(
                            clientInfo.Handle,
                            marketBaitCenterX,
                            marketBaitCenterY,
                            fastMove: false,
                            cancellationToken: cancellationToken);

                        BotLogger.LogInfo(clientInfo.Id, $"Yem satın alındı ({buyIdx}/{emptyCount}). 2 saniye bekleniyor...");
                        await Task.Delay(2000, cancellationToken);
                    }

                    if (cancellationToken.IsCancellationRequested) break;

                    // C: InventoryPosition içerisindeki tüm yemler tekrardan stacklenecek ve InventoryBaitArea içerisindeki uygun yerlere tekrar dizilecek.
                    BotLogger.LogInfo(clientInfo.Id, $"[Market Döngüsü #{cycle}] Yemler stackleniyor ve InventoryBaitArea'ya düzenleniyor...");
                    await StartupBaitOrganizerFunction.StackInventoryBaitsAsync(clientInfo, cancellationToken);
                    await Task.Delay(300, cancellationToken);

                    await StartupBaitOrganizerFunction.MoveBaitsToBottomEmptySlotsAsync(clientInfo, cancellationToken);
                    await Task.Delay(300, cancellationToken);

                    // D: A Adımına geri dönülecek
                }
            }

            // =========================================================================
            // 2. BÖLÜM: KAMP ATEŞİ KONTROLÜ VE SATIN ALMA (İLK 3 SLOT)
            // =========================================================================
            if (settings.BuyCampfireEnabled && !cancellationToken.IsCancellationRequested)
            {
                await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

                int emptyFireCount = 0;
                using (Bitmap? baitAreaBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryBaitArea))
                {
                    if (baitAreaBmp != null)
                    {
                        emptyFireCount = CountEmptyFirstThreeSlots(baitAreaBmp);
                    }
                }

                BotLogger.LogInfo(clientInfo.Id, $"[Kamp Ateşi Kontrolü] İlk 3 slotta {emptyFireCount} adet boş slot tespit edildi.");

                if (emptyFireCount > 0)
                {
                    int fireBuyCount = emptyFireCount * 2;
                    int marketFireCenterX = RegionConstants.MarketFirePosition.StartX + (RegionConstants.MarketFirePosition.Width / 2);
                    int marketFireCenterY = RegionConstants.MarketFirePosition.StartY + (RegionConstants.MarketFirePosition.Height / 2);

                    BotLogger.LogInfo(clientInfo.Id, $"Marketteki Kamp Ateşine ({marketFireCenterX}, {marketFireCenterY}) 1 sn aralıkla {fireBuyCount} defa sağ tıklanıyor (Boşluk: {emptyFireCount} x 2)...");

                    for (int fireIdx = 1; fireIdx <= fireBuyCount; fireIdx++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        await HumanMouseService.Instance.RightClickLocalAsync(
                            clientInfo.Handle,
                            marketFireCenterX,
                            marketFireCenterY,
                            fastMove: false,
                            cancellationToken: cancellationToken);

                        BotLogger.LogInfo(clientInfo.Id, $"Kamp ateşi satın alındı ({fireIdx}/{fireBuyCount}). 1 saniye bekleniyor...");
                        await Task.Delay(1000, cancellationToken);
                    }

                    // Satın alınan ateşleri ilk 3 slota yerleştir
                    BotLogger.LogInfo(clientInfo.Id, "Satın alınan kamp ateşleri ilk 3 slota yerleştiriliyor...");
                    await StartupBaitOrganizerFunction.OrganizeCampfiresToFirstThreeSlotsAsync(clientInfo, cancellationToken);
                    await Task.Delay(300, cancellationToken);
                }
                else
                {
                    BotLogger.LogSuccess(clientInfo.Id, "✅ İlk 3 slotta kamp ateşleri zaten mevcut, satın almaya gerek yok.");
                }
            }
            else if (!settings.BuyCampfireEnabled)
            {
                BotLogger.LogInfo(clientInfo.Id, "BuyCampfire ayarı pasif (kapalı) olduğu için kamp ateşi kontrolü ve satın alma adımı atlandı.");
            }

            // =========================================================================
            // TÜM İŞLEMLER TAMAMLANDIĞINDA: MarketExitButtonPosition'a tıkla
            // =========================================================================
            if (!cancellationToken.IsCancellationRequested)
            {
                int exitX = RegionConstants.MarketExitButtonPosition.StartX + (RegionConstants.MarketExitButtonPosition.Width / 2);
                int exitY = RegionConstants.MarketExitButtonPosition.StartY + (RegionConstants.MarketExitButtonPosition.Height / 2);

                BotLogger.LogInfo(clientInfo.Id, $"Tüm market işlemleri tamamlandı. Market kapatılıyor: MarketExitButtonPosition ({exitX}, {exitY}) tıklanıyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(
                    clientInfo.Handle,
                    exitX,
                    exitY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                BotLogger.LogSuccess(clientInfo.Id, "✅ Market penceresi kapatıldı.");
                await Task.Delay(500, cancellationToken);
            }
        }

        /// <summary>
        /// InventoryBaitArea içerisindeki ilk 3 slotta (Slot 1, 2, 3) yalnızca EmptySlot şablonu ile eşleşen
        /// (gerçekten boş olan) slot sayısını döndürür. Slotta başka bir nesne varsa dolu kabul edilir ve dahil edilmez.
        /// </summary>
        private static int CountEmptyFirstThreeSlots(Bitmap baitAreaBmp)
        {
            var emptyMatches = TemplateConstants.MatchAll(baitAreaBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);

            int colWidth = RegionConstants.InventoryBaitArea.Width / 5; // ~34 px
            int rowHeight = RegionConstants.InventoryBaitArea.Height / 2; // ~37 px

            bool[] isSlotEmpty = new bool[3];

            foreach (var match in emptyMatches)
            {
                int cx = match.Location.X + (match.Bounds.Width / 2);
                int cy = match.Location.Y + (match.Bounds.Height / 2);

                // 1. Satırda mı? (İlk 3 slot)
                if (cy < rowHeight + 5)
                {
                    int colIndex = cx / colWidth;
                    if (colIndex >= 0 && colIndex < 3)
                    {
                        isSlotEmpty[colIndex] = true;
                    }
                }
            }

            int emptyCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (isSlotEmpty[i]) emptyCount++;
            }

            return emptyCount;
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
        /// Koordinatın InventoryBaitArea'nın ilk 3 slotuna (1. satır, ilk 3 sütun) ait olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsFirstThreeSlotsOfBaitAreaDirect(int localXInBaitArea, int localYInBaitArea)
        {
            int columnWidth = RegionConstants.InventoryBaitArea.Width / 5; // ~34 px
            int rowHeight = RegionConstants.InventoryBaitArea.Height / 2;  // ~37 px

            // 1. Satırda mı?
            bool isFirstRow = localYInBaitArea < (rowHeight + 5);

            // İlk 3 sütunda mı? (Sütun 1, 2, 3)
            bool isFirstThreeCols = localXInBaitArea < (columnWidth * 3 + 5);

            return isFirstRow && isFirstThreeCols;
        }
    }
}
