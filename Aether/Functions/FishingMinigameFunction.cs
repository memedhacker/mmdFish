using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aether.Functions
{
    /// <summary>
    /// Balık tutma mini oyununu yöneten yüksek performanslı modüler motor.
    /// Çember pembe rengini (#FFADC7) 4 kontrol noktasında tarar,
    /// balık çember içerisine girdiğinde balık piksellerini (fishTargetColorValues) tespit eder,
    /// 5-10ms çevik fare mikro hareketiyle tıklar ve her pembe döngüsünde kesinlikle 1 tıklama yapılmasını garanti eder.
    /// </summary>
    public static class FishingMinigameFunction
    {
        #region Balık Hedef Renk Değerleri (ARGB 32-bit Negatif Değerler)

        private static readonly int[] FishTargetColorValues = new int[]
        {
            -13412486, -12885891, -13149573, -12886405, -13017734,
            -13017989, -12886919, -13018247, -13017991, -13016706,
            -13478021, -13018248, -10329738, -13084038, -13084039,
            -12952455, -12952712
        };

        private static readonly HashSet<int> FishTargetColorSet = new(FishTargetColorValues);

        #endregion

        #region FishCircleArea İçindeki Göreli Kontrol Bölgeleri

        /// <summary>
        /// FishCircleArea (154, 111 -> 325, 261) ekran görüntüsü üzerindeki
        /// 4 adet CircleColorControlArea alt dikdörtgen koordinatları.
        /// </summary>
        private static readonly Rectangle[] RelativeCircleControlAreas = new[]
        {
            new Rectangle(
                RegionConstants.CircleColorControlArea1.StartX - RegionConstants.FishCircleArea.StartX,
                RegionConstants.CircleColorControlArea1.StartY - RegionConstants.FishCircleArea.StartY,
                RegionConstants.CircleColorControlArea1.Width,
                RegionConstants.CircleColorControlArea1.Height),

            new Rectangle(
                RegionConstants.CircleColorControlArea2.StartX - RegionConstants.FishCircleArea.StartX,
                RegionConstants.CircleColorControlArea2.StartY - RegionConstants.FishCircleArea.StartY,
                RegionConstants.CircleColorControlArea2.Width,
                RegionConstants.CircleColorControlArea2.Height),

            new Rectangle(
                RegionConstants.CircleColorControlArea3.StartX - RegionConstants.FishCircleArea.StartX,
                RegionConstants.CircleColorControlArea3.StartY - RegionConstants.FishCircleArea.StartY,
                RegionConstants.CircleColorControlArea3.Width,
                RegionConstants.CircleColorControlArea3.Height),

            new Rectangle(
                RegionConstants.CircleColorControlArea4.StartX - RegionConstants.FishCircleArea.StartX,
                RegionConstants.CircleColorControlArea4.StartY - RegionConstants.FishCircleArea.StartY,
                RegionConstants.CircleColorControlArea4.Width,
                RegionConstants.CircleColorControlArea4.Height)
        };

        #endregion

        /// <summary>
        /// Balık tutma mini oyun döngüsünü arka planda yüksek FPS ile çalıştırır.
        /// A: CircleColorControlArea1-4 içinde #FFADC7 ara -> bulunursa B'ye geç.
        /// B: FishCircleArea içinde balık renklerini (fishTargetColorValues) ara.
        /// C: Hedef tespit edildiğinde fareyi hedefe taşı, koordinata ulaştığından emin olup tıkla.
        /// D: Çember pembe kaldığı sürece 100ms aralıkla tekrar tıklanabilir; pembeden çıktığı anda kilit sıfırlanıp A adımına dönülür.
        /// </summary>
        /// <param name="clientInfo">İlgili istemci bilgisi</param>
        /// <param name="cancellationToken">İptal isteği bayrağı</param>
        public static async Task ExecuteMinigameAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle)) return;

            BotLogger.LogInfo(clientInfo.Id, "🎮 Balık yakalama mini oyunu başlatıldı.");

            bool hasClickedCurrentPinkCycle = false;
            var clickStopwatch = new System.Diagnostics.Stopwatch();
            int totalFishClicks = 0;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // FishCircleArea (Tüm çemberi ve 4 kontrol bölgesini kapsayan alan) yakalanır
                    using (Bitmap? circleBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.FishCircleArea))
                    {
                        if (circleBmp == null)
                        {
                            await Task.Delay(5, cancellationToken);
                            continue;
                        }

                        // A ADIMI: 4 kontrol bölgesinde #FFADC7 (Pembe) rengini ara
                        bool isPinkPresent = CheckPinkCircleInBmp(circleBmp);

                        if (isPinkPresent)
                        {
                            // B & C ADIMI: İlk kez pembeye döndüyse VEYA 100ms boyunca hala pembeyse tekrar tıklanabilir
                            bool canClick = !hasClickedCurrentPinkCycle || (clickStopwatch.IsRunning && clickStopwatch.ElapsedMilliseconds >= 100);

                            if (canClick)
                            {
                                if (TryFindFishTarget(circleBmp, out int targetLocalX, out int targetLocalY))
                                {
                                    totalFishClicks++;
                                    BotLogger.LogSuccess(clientInfo.Id, $"🎯 Balık yakalandı (#{totalFishClicks}, Konum: {targetLocalX}, {targetLocalY})! Tıklanıyor...");

                                    // Hedefe tam oturduğundan emin olduktan sonra tıklama
                                    await QuickAimAndClickLocalAsync(clientInfo.Handle, targetLocalX, targetLocalY, cancellationToken);

                                    hasClickedCurrentPinkCycle = true;
                                    clickStopwatch.Restart();
                                }
                            }
                        }
                        else
                        {
                            // D ADIMI: Halka artık pembe değil (#FFADC7 kayboldu). Kilidi aç ve sıfırla (A adımına dön)
                            if (hasClickedCurrentPinkCycle)
                            {
                                hasClickedCurrentPinkCycle = false;
                                clickStopwatch.Reset();
                            }
                        }
                    }

                    // Ultra düşük gecikmeli döngü beklemesi (~3ms)
                    await Task.Delay(3, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Görev iptal edildiğinde beklenen temiz çıkış
            }
            finally
            {
                BotLogger.LogInfo(clientInfo.Id, $"🎮 Balık yakalama mini oyunu tamamlandı. (Toplam Yapılan Tıklama: {totalFishClicks})");
            }
        }

        #region Yüksek Hızlı Bellek Taramaları (Unsafe Memory Scanning)

        /// <summary>
        /// 4 kontrol alanı içerisinde #FFADC7 (Pembe) rengin bulunup bulunmadığını doğrudan RAM üzerinden tarar.
        /// </summary>
        private static bool CheckPinkCircleInBmp(Bitmap screenshotBmp)
        {
            BitmapData bmpData = screenshotBmp.LockBits(
                new Rectangle(0, 0, screenshotBmp.Width, screenshotBmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)bmpData.Scan0;
                    int stride = bmpData.Stride;

                    foreach (var rect in RelativeCircleControlAreas)
                    {
                        int startX = Math.Max(0, rect.X);
                        int startY = Math.Max(0, rect.Y);
                        int endX = Math.Min(screenshotBmp.Width, rect.Right);
                        int endY = Math.Min(screenshotBmp.Height, rect.Bottom);

                        for (int y = startY; y < endY; y++)
                        {
                            byte* row = scan0 + (y * stride);
                            for (int x = startX; x < endX; x++)
                            {
                                byte* px = row + (x * 4);
                                byte b = px[0];
                                byte g = px[1];
                                byte r = px[2];

                                // #FFADC7: R=255, G=173, B=199 (Anti-aliasing toleransı dahil)
                                if (r >= 245 && g >= 160 && g <= 186 && b >= 186 && b <= 212)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            finally
            {
                screenshotBmp.UnlockBits(bmpData);
            }
        }

        /// <summary>
        /// FishCircleArea görseli içerisinde balık hedef renklerinden (FishTargetColorSet) herhangi biriyle eşleşen pikseli arar.
        /// Eski ScanAreaForFishColorsAsync fonksiyonundaki doğrusal pointer ilerletme (currentPixelPointer += 4) stili
        /// kullanılarak çarpma işlemi ortadan kaldırılır ve tarama hızı maksimize edilir.
        /// </summary>
        private static bool TryFindFishTarget(Bitmap screenshotBmp, out int localX, out int localY)
        {
            localX = -1;
            localY = -1;

            BitmapData bmpData = screenshotBmp.LockBits(
                new Rectangle(0, 0, screenshotBmp.Width, screenshotBmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                bool isColorFound = false;
                int foundX = -1;
                int foundY = -1;

                unsafe
                {
                    byte* currentPixelPointer = (byte*)bmpData.Scan0;
                    int width = screenshotBmp.Width;
                    int height = screenshotBmp.Height;
                    int stride = bmpData.Stride;
                    int rowPadding = stride - (width * 4); // Satır sonundaki padding byte farkı

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            byte currentBlue = currentPixelPointer[0];
                            byte currentGreen = currentPixelPointer[1];
                            byte currentRed = currentPixelPointer[2];
                            byte currentAlpha = currentPixelPointer[3];

                            // 4 byte veriyi tek bir 32-bit int içine bit kaydırmayla paketliyoruz
                            int currentPixelColorAsInt = (currentAlpha << 24) | (currentRed << 16) | (currentGreen << 8) | currentBlue;

                            // Negatif renk listemizdeki değerlerden biriyle çakıştı mı?
                            if (FishTargetColorSet.Contains(currentPixelColorAsInt))
                            {
                                foundX = x;
                                foundY = y;
                                isColorFound = true;
                                break;
                            }

                            currentPixelPointer += 4;
                        }
                        if (isColorFound) break;
                        currentPixelPointer += rowPadding; // Satır padding'ini atla
                    }
                }

                if (isColorFound)
                {
                    localX = RegionConstants.FishCircleArea.StartX + foundX;
                    localY = RegionConstants.FishCircleArea.StartY + foundY;
                }

                return isColorFound;
            }
            finally
            {
                screenshotBmp.UnlockBits(bmpData);
            }
        }

        #endregion

        #region Hızlı Fare Yönlendirme ve Tıklama (Direct Fast Click)

        /// <summary>
        /// Fareyi hedef balık koordinatına taşır, hedefe ulaştığını kesinleştirdikten sonra donanımsal sol tıklar.
        /// </summary>
        private static async Task QuickAimAndClickLocalAsync(IntPtr hWnd, int localX, int localY, CancellationToken cancellationToken)
        {
            Point targetScreen = HumanMouseService.LocalToScreen(hWnd, localX, localY);

            // Doğrudan hedef koordinata git
            Win32Native.SetCursorPos(targetScreen.X, targetScreen.Y);

            // Fare imlecinin hedefe oturduğundan kesinlikle emin ol
            await Task.Delay(8, cancellationToken);
            if (Cursor.Position.X != targetScreen.X || Cursor.Position.Y != targetScreen.Y)
            {
                Win32Native.SetCursorPos(targetScreen.X, targetScreen.Y);
                await Task.Delay(2, cancellationToken);
            }

            // Donanımsal Sol Tıklama
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            await Task.Delay(15, cancellationToken);
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        #endregion
    }
}
