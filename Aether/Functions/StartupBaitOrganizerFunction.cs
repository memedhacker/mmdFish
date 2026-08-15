using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Envanterdeki yemleri (solucan/paket) tarayan, birbiri üstüne sürükleyerek stackleyen
    /// ve envanterin en altındaki boş slotlara düzenleyen modül.
    /// </summary>
    public static class StartupBaitOrganizerFunction
    {
        /// <summary>
        /// Envanterdeki yemleri (yem.png) >= %60.0 benzerlik eşiği ile tarar ve birleştirir.
        /// Her sürükle-bıraktan sonra sürüklenen yemin ve hedef slotun değişimini piksel seviyesinde kontrol eder.
        /// Eğer sürükle-bıraktan sonra slot tıpatıp aynı kalmışsa, hedef slotun stack sınırına (200) ulaştığı anlaşılır
        /// ve bir daha o slotun üzerine yem sürüklenmez.
        /// </summary>
        public static async Task StackInventoryBaitsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            BotLogger.LogInfo(clientInfo.Id, "Yemler taranıyor ve birleştiriliyor (Stack)...");

            // 1. Tarama öncesinde fareyi envanter alanının dışına çek
            await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // 2. İlk tarama
            Bitmap? currentBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPosition);
            if (currentBmp == null)
            {
                BotLogger.LogError(clientInfo.Id, "Envanter bölgesi ekran görüntüsü alınamadı.");
                return;
            }

            var currentMatches = TemplateConstants.MatchAll(currentBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60);

            // Eğer match sayısı en başta 1 veya daha az ise döngüye girme
            if (currentMatches.Count <= 1)
            {
                BotLogger.LogInfo(clientInfo.Id, $"Envanterde {currentMatches.Count} adet yem bulundu. Birleştirme döngüsüne gerek yok.");
                currentBmp.Dispose();
                return;
            }

            BotLogger.LogInfo(clientInfo.Id, $"Başlangıçta {currentMatches.Count} adet ayrı yem slotu bulundu. Birleştirme başlatılıyor...");

            // Stack sınırına (200) ulaşmış slotların koordinatlarını tutan liste
            var fullStackSlots = new HashSet<Point>();
            int maxOperations = 25;
            int opCount = 0;

            while (opCount < maxOperations && !cancellationToken.IsCancellationRequested)
            {
                opCount++;

                // Sınırına ulaşmamış (henüz yem kabul edebilecek) hedef slotları filtrele
                var availableTargets = currentMatches
                    .Where(m => !fullStackSlots.Any(f => Math.Abs(f.X - m.Location.X) < 16 && Math.Abs(f.Y - m.Location.Y) < 16))
                    .ToList();

                // Eğer birleştirilebilecek hedef slot kalmadıysa veya toplam yem slotu <= 1 ise bitir
                if (availableTargets.Count == 0 || currentMatches.Count <= 1)
                {
                    BotLogger.LogSuccess(clientInfo.Id, $"Yem stackleme tamamlandı. Birleştirilebilecek başka slot kalmadı (Mevcut slot: {currentMatches.Count}).");
                    break;
                }

                // Hedef: mevcut en baştaki uygun yem slotu
                var targetMatch = availableTargets[0];

                // Kaynak: Hedef slottan farklı olan en sondaki yem slotu
                var sourceMatch = currentMatches.LastOrDefault(m => Math.Abs(m.Location.X - targetMatch.Location.X) >= 16 || Math.Abs(m.Location.Y - targetMatch.Location.Y) >= 16);

                if (sourceMatch == null)
                {
                    BotLogger.LogInfo(clientInfo.Id, "Birleştirilecek kaynak yem slotu kalmadı.");
                    break;
                }

                int fromLocalX = RegionConstants.InventoryPosition.StartX + sourceMatch.Location.X + (sourceMatch.Bounds.Width / 2);
                int fromLocalY = RegionConstants.InventoryPosition.StartY + sourceMatch.Location.Y + (sourceMatch.Bounds.Height / 2);

                int toLocalX = RegionConstants.InventoryPosition.StartX + targetMatch.Location.X + (targetMatch.Bounds.Width / 2);
                int toLocalY = RegionConstants.InventoryPosition.StartY + targetMatch.Location.Y + (targetMatch.Bounds.Height / 2);

                Debug.WriteLine($"[StartupBaitOrganizer] Yem sürükleniyor: ({fromLocalX}, {fromLocalY}) -> ({toLocalX}, {toLocalY})");

                // İnsansı kavisle sürükle ve bırak
                await HumanMouseService.Instance.DragAndDropLocalAsync(
                    clientInfo.Handle,
                    fromLocalX,
                    fromLocalY,
                    toLocalX,
                    toLocalY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                // Sürükle-bıraktan sonra 150-250ms bekle
                await Task.Delay(Random.Shared.Next(150, 251), cancellationToken);

                // Fareyi envanter dışına çek
                await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

                // Yeni ekran görüntüsü al
                Bitmap? stepBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPosition);
                if (stepBmp == null) break;

                var newMatches = TemplateConstants.MatchAll(stepBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60);

                // Sürüklenen yemin ve hedef slotun değişimini kontrol et
                bool sourceSlotUnchanged = IsSlotIdentical(currentBmp, stepBmp, sourceMatch.Bounds);
                bool targetSlotUnchanged = IsSlotIdentical(currentBmp, stepBmp, targetMatch.Bounds);
                bool countDecreased = newMatches.Count < currentMatches.Count;

                if (countDecreased || !sourceSlotUnchanged)
                {
                    // Başarıyla birleşti veya kaynak slot boşaldı
                    BotLogger.LogSuccess(clientInfo.Id, $"Yem birleştirildi ({newMatches.Count} yem slotu kaldı).");
                }
                else if (sourceSlotUnchanged && targetSlotUnchanged)
                {
                    // Sürükle-bıraktan sonra hala tıpatıp aynı -> Hedef slot stack sınırına (200) gelmiş!
                    BotLogger.LogWarning(clientInfo.Id, $"Hedef slot stack sınırına (200) ulaştı. Bir daha bu slota yem sürüklenmeyecek.");
                    fullStackSlots.Add(targetMatch.Location);
                }

                currentBmp.Dispose();
                currentBmp = stepBmp;
                currentMatches = newMatches;

                await Task.Delay(Random.Shared.Next(150, 251), cancellationToken);
            }

            currentBmp?.Dispose();
        }

        /// <summary>
        /// İki bitmap üzerindeki belirli bir slot bölgesinin piksel piksel tıpatıp aynı olup olmadığını kontrol eder.
        /// </summary>
        private static bool IsSlotIdentical(Bitmap bmp1, Bitmap bmp2, Rectangle bounds)
        {
            if (bmp1 == null || bmp2 == null) return false;

            int startX = Math.Max(0, bounds.X);
            int startY = Math.Max(0, bounds.Y);
            int width = Math.Min(bounds.Width, Math.Min(bmp1.Width - startX, bmp2.Width - startX));
            int height = Math.Min(bounds.Height, Math.Min(bmp1.Height - startY, bmp2.Height - startY));

            if (width <= 0 || height <= 0) return false;

            int diffCount = 0;
            int totalChecked = 0;

            for (int y = startY; y < startY + height; y += 2)
            {
                for (int x = startX; x < startX + width; x += 2)
                {
                    totalChecked++;
                    Color c1 = bmp1.GetPixel(x, y);
                    Color c2 = bmp2.GetPixel(x, y);

                    int diff = Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B);
                    if (diff > 35)
                    {
                        diffCount++;
                    }
                }
            }

            if (totalChecked == 0) return false;

            // Eğer piksellerin %95'inden fazlası birebir aynıysa slot tıpatıp aynıdır
            double matchRatio = 1.0 - ((double)diffCount / totalChecked);
            return matchRatio >= 0.95;
        }

        /// <summary>
        /// Tüm envanterde (InventoryPosition) Ates (ates.png) şablonunu arar ve bulunan tüm ateşleri
        /// InventoryBaitArea'nın ilk 3 slotuna (Slot 1, 2, 3) taşır. 3'ten fazla ateş varsa üst üste stackler.
        /// </summary>
        public static async Task OrganizeCampfiresToFirstThreeSlotsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            Debug.WriteLine($"[StartupBaitOrganizer] Client #{clientInfo.Id} -> Kamp Ateşleri (ates.png) taranıyor ve ilk 3 slota yerleştiriliyor...");

            // 1. Tarama öncesi fareyi envanter dışına çek
            await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            // İlk 3 slotun merkez koordinatları (Slot 1, Slot 2, Slot 3)
            Point[] firstThreeSlots = new Point[]
            {
                new Point(644, 482), // 1. Slot
                new Point(678, 482), // 2. Slot
                new Point(713, 482)  // 3. Slot
            };

            int maxMoves = 15;
            int moveCount = 0;

            while (moveCount < maxMoves && !cancellationToken.IsCancellationRequested)
            {
                using Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPosition);
                if (invBmp == null)
                {
                    Debug.WriteLine("[StartupBaitOrganizer] Envanter ekran görüntüsü alınamadı.");
                    break;
                }

                // Tüm envanterdeki ateşleri tespit et
                var atesMatches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.80);
                if (atesMatches.Count == 0)
                {
                    Debug.WriteLine("[StartupBaitOrganizer] Envanterde taşınacak Kamp Ateşi (ates.png) bulunamadı.");
                    break;
                }

                // İlk 3 slot DIŞINDA bulunan ateşleri tespit et
                var outsideAtes = atesMatches
                    .Where(a => !IsFirstThreeSlotsOfBaitArea(a.Location.X + (a.Bounds.Width / 2), a.Location.Y + (a.Bounds.Height / 2)))
                    .ToList();

                // Eğer dışarıda hiç ateş kalmadıysa tüm ateşler zaten ilk 3 slotta demektir!
                if (outsideAtes.Count == 0)
                {
                    Debug.WriteLine($"[StartupBaitOrganizer] Tüm Kamp Ateşleri ({atesMatches.Count} adet) başarıyla ilk 3 slota yerleştirildi.");
                    break;
                }

                // İlk 3 slotun hangilerinde şu an ateş var kontrol et
                bool[] slotHasAtes = new bool[3];
                foreach (var ates in atesMatches)
                {
                    int centerX = ates.Location.X + (ates.Bounds.Width / 2);
                    int centerY = ates.Location.Y + (ates.Bounds.Height / 2);

                    if (IsFirstThreeSlotsOfBaitArea(centerX, centerY))
                    {
                        // Hangi sütun (0, 1, 2)?
                        int baitAreaRelStartX = RegionConstants.InventoryBaitArea.StartX - RegionConstants.InventoryPosition.StartX; // ~4
                        int colWidth = RegionConstants.InventoryBaitArea.Width / 5; // ~34
                        int colIndex = Math.Clamp((centerX - baitAreaRelStartX) / colWidth, 0, 2);
                        slotHasAtes[colIndex] = true;
                    }
                }

                // Hedef slotu seç: Önce boş olan slot 1..3'ü seç, hepsi doluysa sırayla üst üste koy
                Point targetSlot;
                if (!slotHasAtes[0]) targetSlot = firstThreeSlots[0];
                else if (!slotHasAtes[1]) targetSlot = firstThreeSlots[1];
                else if (!slotHasAtes[2]) targetSlot = firstThreeSlots[2];
                else targetSlot = firstThreeSlots[moveCount % 3]; // 3'ten fazlaysa üst üste bırak

                // Taşınacak ateşi seç
                var sourceAtes = outsideAtes[0];
                int fromLocalX = RegionConstants.InventoryPosition.StartX + sourceAtes.Location.X + (sourceAtes.Bounds.Width / 2);
                int fromLocalY = RegionConstants.InventoryPosition.StartY + sourceAtes.Location.Y + (sourceAtes.Bounds.Height / 2);

                int toLocalX = targetSlot.X;
                int toLocalY = targetSlot.Y;

                Debug.WriteLine($"[StartupBaitOrganizer] Kamp Ateşi ilk 3 slota taşınıyor: ({fromLocalX}, {fromLocalY}) -> ({toLocalX}, {toLocalY})");

                // İnsansı kavisle taşı ve bırak
                await HumanMouseService.Instance.DragAndDropLocalAsync(
                    clientInfo.Handle,
                    fromLocalX,
                    fromLocalY,
                    toLocalX,
                    toLocalY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                moveCount++;

                // 150-250ms bekle
                await Task.Delay(Random.Shared.Next(150, 251), cancellationToken);

                // Sonraki tarama öncesinde fareyi envanter dışına çek
                await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            }
        }

        /// <summary>
        /// Stackleme işlemi bittikten sonra envanterdeki tüm yemleri/solucanları (yem.png ve yem200.png)
        /// InventoryBaitArea bölgesinin 4. slot ve sonrasındaki boşluklarına (emptySlot.png) insansı kavisle sürükleyip bırakır.
        /// İlk 3 slota asla yem yerleştirilmez.
        /// </summary>
        public static async Task MoveBaitsToBottomEmptySlotsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            Debug.WriteLine($"[StartupBaitOrganizer] Client #{clientInfo.Id} -> Yemleri/solucanları 4. slot ve sonrasındaki boşluklara yerleştirme işlemi başlatılıyor...");

            // 1. Tarama öncesi fareyi envanter dışına çek
            await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            int maxMoves = 20; // Sonsuz döngü koruması
            int moveCount = 0;

            while (moveCount < maxMoves && !cancellationToken.IsCancellationRequested)
            {
                using Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPosition);
                if (invBmp == null)
                {
                    Debug.WriteLine("[StartupBaitOrganizer] Envanter ekran görüntüsü alınamadı.");
                    break;
                }

                // Mevcut tüm yemleri tespit et (yem.png ve yem200.png)
                var baitMatches = new List<TemplateMatchResult>();
                baitMatches.AddRange(TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Yem, threshold: 0.60));
                baitMatches.AddRange(TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Yem200, threshold: 0.60));

                // Mevcut tüm boş slotları tespit et (emptySlot.png)
                var allEmptyMatches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.EmptySlot, threshold: 0.80);

                // KURAL: İlk 3 slota ASLA yerleştirme! Yalnızca ilk 3 slotta OLMAYAN boşlukları hedef al (4. slot ve sonrası)
                var validEmptyMatches = allEmptyMatches
                    .Where(e => !IsFirstThreeSlotsOfBaitArea(e.Location.X + (e.Bounds.Width / 2), e.Location.Y + (e.Bounds.Height / 2)))
                    .ToList();

                if (baitMatches.Count == 0 || validEmptyMatches.Count == 0)
                {
                    Debug.WriteLine($"[StartupBaitOrganizer] Taşınacak yem ({baitMatches.Count} adet) veya 4.+ slotta geçerli boş yer ({validEmptyMatches.Count} adet) bulunamadı.");
                    break;
                }

                // Yemleri yukarıdan aşağıya sırala (Üstteki veya ilk 3 slottaki yemler ilk önce taşınsın)
                baitMatches.Sort((a, b) => a.Location.Y != b.Location.Y ? a.Location.Y.CompareTo(b.Location.Y) : a.Location.X.CompareTo(b.Location.X));

                // Hedef boşlukları aşağıdan yukarıya sırala (En alttaki boşluklar ilk hedef olsun)
                validEmptyMatches.Sort((a, b) => a.Location.Y != b.Location.Y ? b.Location.Y.CompareTo(a.Location.Y) : b.Location.X.CompareTo(a.Location.X));

                // Taşınması gereken yemi bul:
                // 1) İlk 3 slotta duran bir yem varsa (kesinlikle 4+ slota taşınmalı), VEYA
                // 2) Kendisinden daha aşağıda geçerli bir boşluk olan yem
                var topBait = baitMatches.FirstOrDefault(b =>
                    IsFirstThreeSlotsOfBaitArea(b.Location.X + (b.Bounds.Width / 2), b.Location.Y + (b.Bounds.Height / 2)) ||
                    validEmptyMatches.Any(e => e.Location.Y > b.Location.Y + 15));

                // Eğer hiçbir yemin taşınmasına gerek kalmadıysa tüm yemler 4. slot ve sonrasına düzgünce yerleşmiştir
                if (topBait == null)
                {
                    Debug.WriteLine("[StartupBaitOrganizer] Tüm yemler/solucanlar 4. slot ve sonrasına başarıyla yerleştirildi (İlk 3 slot korundu).");
                    break;
                }

                // Bu yemin taşınabileceği EN ALTTTAKİ geçerli boşluğu seç
                var bottomEmpty = validEmptyMatches.FirstOrDefault(e =>
                    IsFirstThreeSlotsOfBaitArea(topBait.Location.X + (topBait.Bounds.Width / 2), topBait.Location.Y + (topBait.Bounds.Height / 2)) ||
                    e.Location.Y > topBait.Location.Y + 15);

                if (bottomEmpty == null)
                {
                    Debug.WriteLine("[StartupBaitOrganizer] Uygun hedef boş slot bulunamadı.");
                    break;
                }

                int fromLocalX = RegionConstants.InventoryPosition.StartX + topBait.Location.X + (topBait.Bounds.Width / 2);
                int fromLocalY = RegionConstants.InventoryPosition.StartY + topBait.Location.Y + (topBait.Bounds.Height / 2);

                int toLocalX = RegionConstants.InventoryPosition.StartX + bottomEmpty.Location.X + (bottomEmpty.Bounds.Width / 2);
                int toLocalY = RegionConstants.InventoryPosition.StartY + bottomEmpty.Location.Y + (bottomEmpty.Bounds.Height / 2);

                Debug.WriteLine($"[StartupBaitOrganizer] Yem taşınıyor (4. slot ve sonrasına): ({fromLocalX}, {fromLocalY}) -> ({toLocalX}, {toLocalY})");

                // İnsansı kavisle taşı ve bırak
                await HumanMouseService.Instance.DragAndDropLocalAsync(
                    clientInfo.Handle,
                    fromLocalX,
                    fromLocalY,
                    toLocalX,
                    toLocalY,
                    fastMove: false,
                    cancellationToken: cancellationToken);

                moveCount++;

                // Sürükle bırak sonrası 150-250ms arası rastgele bekle
                await Task.Delay(Random.Shared.Next(150, 251), cancellationToken);

                // Sonraki tarama öncesinde fareyi envanter dışına çek
                await MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);
            }
        }

        /// <summary>
        /// Verilen koordinatın InventoryBaitArea'nın ilk 3 slotuna (1. satır, ilk 3 sütun) ait olup olmadığını kontrol eder.
        /// Kullanıcı kuralı gereği yemler asla bu ilk 3 slota yerleştirilmez; 4. slot ve sonrasına yerleştirilir.
        /// </summary>
        private static bool IsFirstThreeSlotsOfBaitArea(int localXInInv, int localYInInv)
        {
            // InventoryPosition göreli koordinatları
            int baitAreaRelStartY = RegionConstants.InventoryBaitArea.StartY - RegionConstants.InventoryPosition.StartY; // ~229 px
            int baitAreaRelStartX = RegionConstants.InventoryBaitArea.StartX - RegionConstants.InventoryPosition.StartX; // ~4 px
            int columnWidth = RegionConstants.InventoryBaitArea.Width / 5; // ~34 px
            int rowHeight = RegionConstants.InventoryBaitArea.Height / 2; // ~37 px

            // 1. Satırda mı? (InventoryBaitArea'nın üst yarısı)
            bool isFirstRow = localYInInv >= (baitAreaRelStartY - 10) && localYInInv < (baitAreaRelStartY + rowHeight + 5);

            // İlk 3 sütunda mı? (Sütun 1, 2, 3)
            bool isFirstThreeCols = localXInInv < (baitAreaRelStartX + (columnWidth * 3) + 5);

            return isFirstRow && isFirstThreeCols;
        }

        /// <summary>
        /// Fare imlecini InventoryPosition bölgesinin dışına (en fazla 100px uzağına) rastgele bir konuma çeker.
        /// Bu sayede tarama esnasında fare imlecinin veya eşya bilgi kutucuklarının (tooltip) şablon eşleşmesini bozması engellenir.
        /// </summary>
        public static async Task MoveMouseOutsideInventoryAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            if (hWnd == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            // InventoryPosition: StartX = 623, StartY = 235, EndX = 798, EndY = 542
            // Envanterin sol dış tarafında en fazla 100px (20px ile 90px arası) rastgele güvenli bir nokta
            int outsideX = Math.Max(20, RegionConstants.InventoryPosition.StartX - Random.Shared.Next(20, 95));
            int outsideY = Random.Shared.Next(RegionConstants.InventoryPosition.StartY, RegionConstants.InventoryPosition.EndY);

            Point screenPt = HumanMouseService.LocalToScreen(hWnd, outsideX, outsideY);
            await HumanMouseService.Instance.MoveMouseAsync(screenPt.X, screenPt.Y, cancellationToken);

            // Tooltip veya hover efektlerinin ekrandan tamamen silinmesi için kısa insansı bekleme
            await Task.Delay(Random.Shared.Next(60, 100), cancellationToken);
        }
    }
}
