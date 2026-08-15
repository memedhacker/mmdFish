using Aether.Native;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aether.Services
{
    /// <summary>
    /// Windows'un gerçek fiziksel fare imlecini doğrudan kontrol eden,
    /// insan elinin doğal kavislerini (Cubic Bézier), hızlanma-yavaşlama (Ease-in-out)
    /// ve mikro kas titremelerini (Jitter) simüle eden modüler fare otomasyon servisi.
    /// </summary>
    public class HumanMouseService
    {
        private static readonly Lazy<HumanMouseService> _instance = new Lazy<HumanMouseService>(() => new HumanMouseService());
        public static HumanMouseService Instance => _instance.Value;

        private readonly Random _random = new();

        private HumanMouseService()
        {
        }

        #region 1. İnsansı Fare Hareketi (Normal Hız)

        /// <summary>
        /// Windows'un gerçek fare imlecini hedef masaüstü ekran koordinatına (Screen X, Y)
        /// insansı kavisli Bézier eğrisi ve doğal hızlanma-yavaşlama ile taşır.
        /// </summary>
        /// <param name="targetScreenX">Masaüstü ekran hedef X koordinatı</param>
        /// <param name="targetScreenY">Masaüstü ekran hedef Y koordinatı</param>
        /// <param name="cancellationToken">İptal jetonu</param>
        public Task MoveMouseAsync(int targetScreenX, int targetScreenY, CancellationToken cancellationToken = default)
        {
            return MoveInternalAsync(targetScreenX, targetScreenY, isFast: false, cancellationToken);
        }

        /// <summary>
        /// Pencere içi yerel koordinatı (Local X, Y) ekran koordinatına çevirerek
        /// fareyi insansı kavisle hedefe taşır.
        /// </summary>
        public Task MoveMouseToLocalAsync(IntPtr hWnd, int localX, int localY, CancellationToken cancellationToken = default)
        {
            Point screenPt = LocalToScreen(hWnd, localX, localY);
            return MoveMouseAsync(screenPt.X, screenPt.Y, cancellationToken);
        }

        #endregion

        #region 2. Hızlı İnsansı Fare Hareketi (Fast Mouse Move)

        /// <summary>
        /// Fareyi hedef ekrana yine insansı ve kavisli fakat ÇOK DAHA HIZLI (Fast Move) bir şekilde taşır.
        /// İleride acil mini-game tepkileri veya hızlı işlemler için kullanılmak üzere tasarlanmıştır.
        /// </summary>
        /// <param name="targetScreenX">Masaüstü ekran hedef X koordinatı</param>
        /// <param name="targetScreenY">Masaüstü ekran hedef Y koordinatı</param>
        /// <param name="cancellationToken">İptal jetonu</param>
        public Task MoveMouseFastAsync(int targetScreenX, int targetScreenY, CancellationToken cancellationToken = default)
        {
            return MoveInternalAsync(targetScreenX, targetScreenY, isFast: true, cancellationToken);
        }

        /// <summary>
        /// Pencere içi yerel koordinatı (Local X, Y) ekran koordinatına çevirerek
        /// fareyi ÇOK DAHA HIZLI insansı kavisle hedefe taşır.
        /// </summary>
        public Task MoveMouseFastToLocalAsync(IntPtr hWnd, int localX, int localY, CancellationToken cancellationToken = default)
        {
            Point screenPt = LocalToScreen(hWnd, localX, localY);
            return MoveMouseFastAsync(screenPt.X, screenPt.Y, cancellationToken);
        }

        #endregion

        #region 3. Çekirdek İnsansı Bézier Algoritması

        /// <summary>
        /// Cubic Bézier + EaseInOutCubic + MicroJitter tabanlı gerçek Windows imleç hareketi.
        /// </summary>
        private async Task MoveInternalAsync(int targetScreenX, int targetScreenY, bool isFast, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Point startScreenPos = Cursor.Position;

            // Hedef koordinata ufak insansı varyasyon (±1 px)
            int finalTargetX = targetScreenX + _random.Next(-1, 2);
            int finalTargetY = targetScreenY + _random.Next(-1, 2);

            double distance = Math.Sqrt(Math.Pow(finalTargetX - startScreenPos.X, 2) + Math.Pow(finalTargetY - startScreenPos.Y, 2));
            if (distance < 2)
            {
                Win32Native.SetCursorPos(finalTargetX, finalTargetY);
                return;
            }

            // Adım sayısı ve süre belirleme
            int steps;
            int baseDurationMs;

            if (isFast)
            {
                // Hızlı mod: 60 - 180 ms arası çevik tepki
                steps = Math.Clamp((int)(distance / 18.0), 6, 18);
                baseDurationMs = Math.Clamp((int)(distance * 0.35) + _random.Next(30, 60), 60, 180);
            }
            else
            {
                // Normal insansı mod: 200 - 550 ms arası doğal insan hareketi
                steps = Math.Clamp((int)(distance / 7.0), 16, 45);
                baseDurationMs = Math.Clamp((int)(distance * 0.80) + _random.Next(100, 180), 180, 500);
            }

            // Kübik Bézier Kontrol Noktaları (Cubic Bézier Control Points)
            double curveOffsetFactor = isFast ? 0.15 : 0.26;
            double maxOffset = distance * curveOffsetFactor;

            double p1OffsetX = (_random.NextDouble() * 2.0 - 1.0) * maxOffset;
            double p1OffsetY = (_random.NextDouble() * 2.0 - 1.0) * maxOffset;
            double p2OffsetX = (_random.NextDouble() * 2.0 - 1.0) * maxOffset * 0.7;
            double p2OffsetY = (_random.NextDouble() * 2.0 - 1.0) * maxOffset * 0.7;

            PointF p0 = startScreenPos;
            PointF p1 = new PointF((float)(startScreenPos.X + (finalTargetX - startScreenPos.X) * 0.33 + p1OffsetX),
                                   (float)(startScreenPos.Y + (finalTargetY - startScreenPos.Y) * 0.33 + p1OffsetY));
            PointF p2 = new PointF((float)(startScreenPos.X + (finalTargetX - startScreenPos.X) * 0.66 + p2OffsetX),
                                   (float)(startScreenPos.Y + (finalTargetY - startScreenPos.Y) * 0.66 + p2OffsetY));
            PointF p3 = new PointF(finalTargetX, finalTargetY);

            int stepDelay = Math.Max(3, baseDurationMs / steps);

            // Windows fare imlecini kavis boyunca SetCursorPos ile adım adım hareket ettir
            for (int i = 1; i <= steps; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                double t = (double)i / steps;
                double easedT = EaseInOutCubic(t);

                // Kübik Bézier formülü: B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3
                double oneMinusT = 1.0 - easedT;
                double bx = Math.Pow(oneMinusT, 3) * p0.X +
                            3 * Math.Pow(oneMinusT, 2) * easedT * p1.X +
                            3 * oneMinusT * Math.Pow(easedT, 2) * p2.X +
                            Math.Pow(easedT, 3) * p3.X;

                double by = Math.Pow(oneMinusT, 3) * p0.Y +
                            3 * Math.Pow(oneMinusT, 2) * easedT * p1.Y +
                            3 * oneMinusT * Math.Pow(easedT, 2) * p2.Y +
                            Math.Pow(easedT, 3) * p3.Y;

                // Mikro kas titreşimi (Micro Jitter)
                if (i > 1 && i < steps)
                {
                    bx += (_random.NextDouble() - 0.5) * 1.2;
                    by += (_random.NextDouble() - 0.5) * 1.2;
                }

                Win32Native.SetCursorPos((int)Math.Round(bx), (int)Math.Round(by));
                await Task.Delay(stepDelay, cancellationToken);
            }

            // Son noktaya oturt
            Win32Native.SetCursorPos(finalTargetX, finalTargetY);
        }

        #endregion

        #region 4. Tıklama İşlemleri (Click Actions)

        /// <summary>
        /// Fareyi hedef ekran koordinatına insansı kavisle taşır ve donanımsal sol tıklama (Left Click) yapar.
        /// İsteğe bağlı olarak art arda birden fazla tıklama (clickCount) yapabilir.
        /// </summary>
        public async Task LeftClickAsync(int targetScreenX, int targetScreenY, bool fastMove = false, int clickCount = 1, CancellationToken cancellationToken = default)
        {
            if (fastMove)
                await MoveMouseFastAsync(targetScreenX, targetScreenY, cancellationToken);
            else
                await MoveMouseAsync(targetScreenX, targetScreenY, cancellationToken);

            int totalClicks = Math.Max(1, clickCount);
            for (int i = 0; i < totalClicks; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                // Tıklama öncesi doğal insansı duraksama (Pre-click hesitation)
                await Task.Delay(_random.Next(35, 65), cancellationToken);

                Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                await Task.Delay(_random.Next(40, 75), cancellationToken);
                Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                if (i < totalClicks - 1)
                {
                    // Tıklamalar arası doğal insansı aralık
                    await Task.Delay(_random.Next(50, 110), cancellationToken);
                }
            }
        }

        public Task LeftClickAsync(int targetScreenX, int targetScreenY, bool fastMove, CancellationToken cancellationToken)
            => LeftClickAsync(targetScreenX, targetScreenY, fastMove, 1, cancellationToken);

        public Task LeftClickAsync(int targetScreenX, int targetScreenY, CancellationToken cancellationToken)
            => LeftClickAsync(targetScreenX, targetScreenY, false, 1, cancellationToken);

        /// <summary>
        /// Pencere içi yerel koordinata insansı kavisle gidip sol tıklar (clickCount destekli).
        /// </summary>
        public Task LeftClickLocalAsync(IntPtr hWnd, int localX, int localY, bool fastMove = false, int clickCount = 1, CancellationToken cancellationToken = default)
        {
            Point screenPt = LocalToScreen(hWnd, localX, localY);
            return LeftClickAsync(screenPt.X, screenPt.Y, fastMove, clickCount, cancellationToken);
        }

        public Task LeftClickLocalAsync(IntPtr hWnd, int localX, int localY, bool fastMove, CancellationToken cancellationToken)
            => LeftClickLocalAsync(hWnd, localX, localY, fastMove, 1, cancellationToken);

        public Task LeftClickLocalAsync(IntPtr hWnd, int localX, int localY, CancellationToken cancellationToken)
            => LeftClickLocalAsync(hWnd, localX, localY, false, 1, cancellationToken);

        /// <summary>
        /// Fareyi hedef ekran koordinatına insansı kavisle taşır ve donanımsal sağ tıklama (Right Click) yapar.
        /// </summary>
        public async Task RightClickAsync(int targetScreenX, int targetScreenY, bool fastMove = false, int clickCount = 1, CancellationToken cancellationToken = default)
        {
            if (fastMove)
                await MoveMouseFastAsync(targetScreenX, targetScreenY, cancellationToken);
            else
                await MoveMouseAsync(targetScreenX, targetScreenY, cancellationToken);

            int totalClicks = Math.Max(1, clickCount);
            for (int i = 0; i < totalClicks; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                await Task.Delay(_random.Next(35, 65), cancellationToken);
                Win32Native.mouse_event(Win32Native.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                await Task.Delay(_random.Next(40, 75), cancellationToken);
                Win32Native.mouse_event(Win32Native.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);

                if (i < totalClicks - 1)
                {
                    await Task.Delay(_random.Next(50, 110), cancellationToken);
                }
            }
        }

        public Task RightClickAsync(int targetScreenX, int targetScreenY, bool fastMove, CancellationToken cancellationToken)
            => RightClickAsync(targetScreenX, targetScreenY, fastMove, 1, cancellationToken);

        public Task RightClickAsync(int targetScreenX, int targetScreenY, CancellationToken cancellationToken)
            => RightClickAsync(targetScreenX, targetScreenY, false, 1, cancellationToken);

        /// <summary>
        /// Pencere içi yerel koordinata insansı kavisle gidip sağ tıklar.
        /// </summary>
        public Task RightClickLocalAsync(IntPtr hWnd, int localX, int localY, bool fastMove = false, int clickCount = 1, CancellationToken cancellationToken = default)
        {
            Point screenPt = LocalToScreen(hWnd, localX, localY);
            return RightClickAsync(screenPt.X, screenPt.Y, fastMove, clickCount, cancellationToken);
        }

        public Task RightClickLocalAsync(IntPtr hWnd, int localX, int localY, bool fastMove, CancellationToken cancellationToken)
            => RightClickLocalAsync(hWnd, localX, localY, fastMove, 1, cancellationToken);

        public Task RightClickLocalAsync(IntPtr hWnd, int localX, int localY, CancellationToken cancellationToken)
            => RightClickLocalAsync(hWnd, localX, localY, false, 1, cancellationToken);

        #endregion

        #region 5. Sürükle ve Bırak (Drag & Drop)

        /// <summary>
        /// Windows faresini başlangıç noktasına taşır, sol tuşu basılı tutarak insansı kavisle hedef noktaya sürükler ve bırakır (Drag & Drop).
        /// </summary>
        public async Task DragAndDropAsync(int startScreenX, int startScreenY, int targetScreenX, int targetScreenY, bool fastMove = false, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return;

            // 1. Başlangıç noktasına git
            if (fastMove)
                await MoveMouseFastAsync(startScreenX, startScreenY, cancellationToken);
            else
                await MoveMouseAsync(startScreenX, startScreenY, cancellationToken);

            await Task.Delay(_random.Next(40, 80), cancellationToken);

            // 2. Sol tuşa bas (Mouse Down)
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            await Task.Delay(_random.Next(50, 90), cancellationToken);

            // 3. Basılı tutarak hedef noktaya insansı kavisle sürükle
            if (fastMove)
                await MoveMouseFastAsync(targetScreenX, targetScreenY, cancellationToken);
            else
                await MoveMouseAsync(targetScreenX, targetScreenY, cancellationToken);

            await Task.Delay(_random.Next(50, 90), cancellationToken);

            // 4. Sol tuşu bırak (Mouse Up)
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            await Task.Delay(_random.Next(60, 120), cancellationToken);
        }

        /// <summary>
        /// Pencere içi yerel koordinatlarla insansı sürükle ve bırak (Drag & Drop) işlemi yapar.
        /// </summary>
        public Task DragAndDropLocalAsync(IntPtr hWnd, int startLocalX, int startLocalY, int targetLocalX, int targetLocalY, bool fastMove = false, CancellationToken cancellationToken = default)
        {
            Point startScreen = LocalToScreen(hWnd, startLocalX, startLocalY);
            Point targetScreen = LocalToScreen(hWnd, targetLocalX, targetLocalY);
            return DragAndDropAsync(startScreen.X, startScreen.Y, targetScreen.X, targetScreen.Y, fastMove, cancellationToken);
        }

        public Task DragAndDropLocalAsync(IntPtr hWnd, int startLocalX, int startLocalY, int targetLocalX, int targetLocalY, CancellationToken cancellationToken)
            => DragAndDropLocalAsync(hWnd, startLocalX, startLocalY, targetLocalX, targetLocalY, false, cancellationToken);

        /// <summary>
        /// Windows faresini başlangıç noktasına taşır, orta tuşu (MouseWheel) basılı tutarak hedef noktaya sürükler ve bırakır.
        /// Oyun içi kamera açısını çevirmek / döndürmek için göreceli hareket (MOUSEEVENTF_MOVE) sinyalleri üretir.
        /// </summary>
        public async Task MiddleDragAndDropAsync(int startScreenX, int startScreenY, int targetScreenX, int targetScreenY, bool fastMove = false, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested) return;

            // 1. Başlangıç noktasına git
            if (fastMove)
                await MoveMouseFastAsync(startScreenX, startScreenY, cancellationToken);
            else
                await MoveMouseAsync(startScreenX, startScreenY, cancellationToken);

            await Task.Delay(_random.Next(50, 90), cancellationToken);

            // 2. Orta tuşa basılı tut (Middle Down)
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
            await Task.Delay(_random.Next(60, 100), cancellationToken);

            // 3. Basılı tutarak hedef noktaya göreceli ve insansı adımlarla sürükle (DirectInput/DirectX uyumlu)
            int totalDeltaX = targetScreenX - startScreenX;
            int totalDeltaY = targetScreenY - startScreenY;
            int steps = Math.Clamp((int)(Math.Sqrt(totalDeltaX * totalDeltaX + totalDeltaY * totalDeltaY) / 4.0), 12, 35);

            int prevX = startScreenX;
            int prevY = startScreenY;

            for (int i = 1; i <= steps; i++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                double progress = (double)i / steps;
                // EaseInOutCubic eğrisi
                double eased = progress < 0.5
                    ? 4 * progress * progress * progress
                    : 1 - Math.Pow(-2 * progress + 2, 3) / 2;

                int currX = (int)(startScreenX + totalDeltaX * eased);
                int currY = (int)(startScreenY + totalDeltaY * eased);

                int dx = currX - prevX;
                int dy = currY - prevY;

                prevX = currX;
                prevY = currY;

                // Hem DirectInput (göreceli) hem Windows Cursor (mutlak) pozisyonunu güncelle
                Win32Native.mouse_event(Win32Native.MOUSEEVENTF_MOVE, (uint)dx, (uint)dy, 0, 0);
                Win32Native.SetCursorPos(currX, currY);

                await Task.Delay(_random.Next(12, 22), cancellationToken);
            }

            await Task.Delay(_random.Next(60, 100), cancellationToken);

            // 4. Orta tuşu bırak (Middle Up)
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
            await Task.Delay(_random.Next(60, 120), cancellationToken);
        }

        /// <summary>
        /// Pencere içi yerel koordinatlarla orta tuş (MouseWheel) sürükle ve bırak işlemi yapar.
        /// </summary>
        public Task MiddleDragAndDropLocalAsync(IntPtr hWnd, int startLocalX, int startLocalY, int targetLocalX, int targetLocalY, bool fastMove = false, CancellationToken cancellationToken = default)
        {
            Point startScreen = LocalToScreen(hWnd, startLocalX, startLocalY);
            Point targetScreen = LocalToScreen(hWnd, targetLocalX, targetLocalY);
            return MiddleDragAndDropAsync(startScreen.X, startScreen.Y, targetScreen.X, targetScreen.Y, fastMove, cancellationToken);
        }

        public Task MiddleDragAndDropLocalAsync(IntPtr hWnd, int startLocalX, int startLocalY, int targetLocalX, int targetLocalY, CancellationToken cancellationToken)
            => MiddleDragAndDropLocalAsync(hWnd, startLocalX, startLocalY, targetLocalX, targetLocalY, false, cancellationToken);

        #endregion

        #region 5. Yardımcı Fonksiyonlar

        /// <summary>
        /// HWND penceresinin iç yerel (Local) koordinatını masaüstü ekran (Screen) koordinatına dönüştürür.
        /// </summary>
        public static Point LocalToScreen(IntPtr hWnd, int localX, int localY)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return new Point(localX, localY);
            }

            Win32Native.POINT pt = new Win32Native.POINT(localX, localY);
            if (Win32Native.ClientToScreen(hWnd, ref pt))
            {
                return new Point(pt.X, pt.Y);
            }

            if (Win32Native.GetWindowRect(hWnd, out Win32Native.RECT winRect))
            {
                return new Point(winRect.Left + localX, winRect.Top + localY);
            }

            return new Point(localX, localY);
        }

        /// <summary>
        /// S-Eğrisi Hızlanma-Yavaşlama Fonksiyonu (Cubic Ease-In-Out).
        /// </summary>
        private static double EaseInOutCubic(double t)
        {
            return t < 0.5
                ? 4 * t * t * t
                : 1 - Math.Pow(-2 * t + 2, 3) / 2;
        }

        #endregion
    }
}
