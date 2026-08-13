using Aether.Native;
using Aether.States;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Aether.Helpers
{
    /// <summary>
    /// HWND pencere tutacağı üzerinden ekran görüntüsü alma ve kaydedip işleme süreçlerini yürüten yardımcı sınıf.
    /// Pencere başka pencerelerin altında, arkasında veya simge durumunda (minimized) olsa dahi yakalamayı destekler.
    /// </summary>
    public static class WindowCaptureHelper
    {
        /// <summary>
        /// Belirtilen HWND (pencere tutacağı) üzerindeki pencerenin SADECE İÇ ALANININ (Client Area) ekran görüntüsünü Bitmap olarak döndürür.
        /// Başlık çubuğu (title bar) ve pencere kenarlıkları (borders) dahil edilmez.
        /// Pencere başka bir pencerenin arkasında veya altta kalsa bile arka planda yakalar.
        /// </summary>
        /// <param name="hWnd">Yakalanacak pencerenin HWND adresi.</param>
        /// <param name="restoreIfIconic">Pencere simge durumundaysa (minimized) odak almadan arka planda açsın mı?</param>
        /// <returns>Başarılıysa Bitmap nesnesi (çağıran kişi Dispose etmelidir), başarısızsa null.</returns>
        public static Bitmap? CaptureWindow(IntPtr hWnd, bool restoreIfIconic = true)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return null;
            }

            bool wasIconic = Win32Native.IsIconic(hWnd);

            // Pencere simge durumundaysa (minimized), kullanıcının odağını çalmadan arka planda göster (SW_SHOWNOACTIVATE)
            if (wasIconic && restoreIfIconic)
            {
                Win32Native.ShowWindow(hWnd, Win32Native.SW_SHOWNOACTIVATE);
                System.Threading.Thread.Sleep(50); // DWM arabelleğinin güncellenmesi için kısa bir bekleme
            }

            // 1. Pencerenin iç alan (Client Area) boyutlarını al
            if (!Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect))
            {
                return null;
            }

            int clientWidth = clientRect.Width;
            int clientHeight = clientRect.Height;

            // 2. Tam pencere (Window Rect) ve ekran ofsetlerini hesapla
            Win32Native.GetWindowRect(hWnd, out Win32Native.RECT windowRect);
            int windowWidth = windowRect.Width;
            int windowHeight = windowRect.Height;

            // İç alanın sol-üst köşesinin ekran koordinatları
            Win32Native.POINT clientScreenPt = new Win32Native.POINT(0, 0);
            Win32Native.ClientToScreen(hWnd, ref clientScreenPt);

            // Pencerenin sol-üst kenarlık kalınlıkları (Offset)
            int offsetX = Math.Max(0, clientScreenPt.X - windowRect.Left);
            int offsetY = Math.Max(0, clientScreenPt.Y - windowRect.Top);

            if (clientWidth <= 0 || clientHeight <= 0)
            {
                clientWidth = windowWidth;
                clientHeight = windowHeight;
                offsetX = 0;
                offsetY = 0;
            }

            if (clientWidth <= 0 || clientHeight <= 0)
            {
                return null;
            }

            Bitmap? resultBitmap = null;

            // YÖNTEM 1: PrintWindow + PW_CLIENTONLY (0x01) & PW_RENDERFULLCONTENT (0x02)
            IntPtr hdcScreen = Win32Native.GetDC(IntPtr.Zero);
            IntPtr hdcMem = Win32Native.CreateCompatibleDC(hdcScreen);
            IntPtr hBitmap = Win32Native.CreateCompatibleBitmap(hdcScreen, clientWidth, clientHeight);
            IntPtr hOldBmp = Win32Native.SelectObject(hdcMem, hBitmap);

            try
            {
                bool printed = Win32Native.PrintWindow(hWnd, hdcMem, Win32Native.PW_CLIENTONLY | Win32Native.PW_RENDERFULLCONTENT);

                if (!printed)
                {
                    printed = Win32Native.PrintWindow(hWnd, hdcMem, Win32Native.PW_CLIENTONLY);
                }

                if (printed)
                {
                    using (Bitmap tempBmp = Image.FromHbitmap(hBitmap))
                    {
                        Bitmap bmp = new Bitmap(tempBmp);
                        if (!IsBitmapBlank(bmp))
                        {
                            resultBitmap = bmp;
                        }
                        else
                        {
                            bmp.Dispose();
                        }
                    }
                }
            }
            catch
            {
                resultBitmap = null;
            }
            finally
            {
                Win32Native.SelectObject(hdcMem, hOldBmp);
                Win32Native.DeleteObject(hBitmap);
                Win32Native.DeleteDC(hdcMem);
                Win32Native.ReleaseDC(IntPtr.Zero, hdcScreen);
            }

            // YÖNTEM 2: Tam Pencere PrintWindow çekip iç alanı kırpma (Crop)
            // (PW_CLIENTONLY bazı eski/özel pencerelerde çalışmadığında tam DWM çizimini alıp kenarlıkları keser)
            if (resultBitmap == null && windowWidth > 0 && windowHeight > 0)
            {
                IntPtr hdcScreen2 = Win32Native.GetDC(IntPtr.Zero);
                IntPtr hdcMem2 = Win32Native.CreateCompatibleDC(hdcScreen2);
                IntPtr hBitmapFull = Win32Native.CreateCompatibleBitmap(hdcScreen2, windowWidth, windowHeight);
                IntPtr hOldBmp2 = Win32Native.SelectObject(hdcMem2, hBitmapFull);

                try
                {
                    bool printedFull = Win32Native.PrintWindow(hWnd, hdcMem2, Win32Native.PW_RENDERFULLCONTENT);
                    if (!printedFull)
                    {
                        printedFull = Win32Native.PrintWindow(hWnd, hdcMem2, 0);
                    }

                    if (printedFull)
                    {
                        using (Bitmap tempFullBmp = Image.FromHbitmap(hBitmapFull))
                        {
                            using (Bitmap fullBmp = new Bitmap(tempFullBmp))
                            {
                                if (!IsBitmapBlank(fullBmp))
                                {
                                    int cropW = Math.Min(clientWidth, fullBmp.Width - offsetX);
                                    int cropH = Math.Min(clientHeight, fullBmp.Height - offsetY);

                                    if (cropW > 0 && cropH > 0 && (offsetX > 0 || offsetY > 0 || cropW < fullBmp.Width || cropH < fullBmp.Height))
                                    {
                                        Rectangle cropRect = new Rectangle(offsetX, offsetY, cropW, cropH);
                                        resultBitmap = fullBmp.Clone(cropRect, PixelFormat.Format32bppArgb);
                                    }
                                    else
                                    {
                                        resultBitmap = new Bitmap(fullBmp);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    resultBitmap?.Dispose();
                    resultBitmap = null;
                }
                finally
                {
                    Win32Native.SelectObject(hdcMem2, hOldBmp2);
                    Win32Native.DeleteObject(hBitmapFull);
                    Win32Native.DeleteDC(hdcMem2);
                    Win32Native.ReleaseDC(IntPtr.Zero, hdcScreen2);
                }
            }

            // YÖNTEM 3: Pencerenin kendi Client DC'sinden BitBlt kopyalama
            if (resultBitmap == null)
            {
                IntPtr hdcScreen3 = Win32Native.GetDC(IntPtr.Zero);
                IntPtr hdcMem3 = Win32Native.CreateCompatibleDC(hdcScreen3);
                IntPtr hBitmapClient = Win32Native.CreateCompatibleBitmap(hdcScreen3, clientWidth, clientHeight);
                IntPtr hOldBmp3 = Win32Native.SelectObject(hdcMem3, hBitmapClient);

                try
                {
                    IntPtr hdcClient = Win32Native.GetDC(hWnd); // GetDC doğrudan Client DC verir
                    if (hdcClient != IntPtr.Zero)
                    {
                        bool blt = Win32Native.BitBlt(hdcMem3, 0, 0, clientWidth, clientHeight, hdcClient, 0, 0, Win32Native.SRCCOPY);
                        Win32Native.ReleaseDC(hWnd, hdcClient);

                        if (blt)
                        {
                            using (Bitmap tempBmp = Image.FromHbitmap(hBitmapClient))
                            {
                                Bitmap bmp = new Bitmap(tempBmp);
                                if (!IsBitmapBlank(bmp))
                                {
                                    resultBitmap = bmp;
                                }
                                else
                                {
                                    bmp.Dispose();
                                }
                            }
                        }
                    }
                }
                catch
                {
                    resultBitmap = null;
                }
                finally
                {
                    Win32Native.SelectObject(hdcMem3, hOldBmp3);
                    Win32Native.DeleteObject(hBitmapClient);
                    Win32Native.DeleteDC(hdcMem3);
                    Win32Native.ReleaseDC(IntPtr.Zero, hdcScreen3);
                }
            }

            // YÖNTEM 4 (Fallback): Ekrandan doğrudan Client Area bölgesini CopyFromScreen ile alma
            if (resultBitmap == null || IsBitmapBlank(resultBitmap))
            {
                try
                {
                    Bitmap fallbackBmp = new Bitmap(clientWidth, clientHeight, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(fallbackBmp))
                    {
                        g.CopyFromScreen(clientScreenPt.X, clientScreenPt.Y, 0, 0, new Size(clientWidth, clientHeight), CopyPixelOperation.SourceCopy);
                    }

                    if (!IsBitmapBlank(fallbackBmp))
                    {
                        resultBitmap?.Dispose();
                        resultBitmap = fallbackBmp;
                    }
                    else if (resultBitmap == null)
                    {
                        resultBitmap = fallbackBmp;
                    }
                    else
                    {
                        fallbackBmp.Dispose();
                    }
                }
                catch
                {
                    // Fallback hata verirse mevcut resultBitmap kalır
                }
            }

            return resultBitmap;
        }

        /// <summary>
        /// Bir görselin tamamen siyah veya boş (transparent/blank) olup olmadığını hızlı örnekleme ile kontrol eder.
        /// </summary>
        private static bool IsBitmapBlank(Bitmap bmp)
        {
            if (bmp == null) return true;
            try
            {
                int w = bmp.Width;
                int h = bmp.Height;
                int stepX = Math.Max(1, w / 10);
                int stepY = Math.Max(1, h / 10);

                for (int x = stepX / 2; x < w; x += stepX)
                {
                    for (int y = stepY / 2; y < h; y += stepY)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        // Siyah veya saydam harici bir renk pikseli varsa görsel boş değildir
                        if (pixel.A > 0 && (pixel.R > 5 || pixel.G > 5 || pixel.B > 5))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ClientState içerisindeki aktif seçili (SelectedClient) pencerenin ekran görüntüsünü Bitmap olarak döndürür.
        /// </summary>
        public static Bitmap? CaptureSelectedClientWindow()
        {
            var selectedClient = ClientState.Instance.SelectedClient;
            if (selectedClient == null || selectedClient.Handle == IntPtr.Zero)
            {
                return null;
            }

            return CaptureWindow(selectedClient.Handle);
        }

        /// <summary>
        /// Verilen HWND penceresinin ekran görüntüsünü alıp Masaüstüne PNG olarak kaydeder.
        /// </summary>
        /// <param name="hWnd">Pencere tutacağı</param>
        /// <param name="fileNamePrefix">Kaydedilecek dosya adı öneki</param>
        /// <returns>Kayıt başarılı ise kaydedilen dosyanın tam yolu, başarısızsa null.</returns>
        public static string? SaveWindowScreenshotToDesktop(IntPtr hWnd, string fileNamePrefix = "Screenshot")
        {
            using (Bitmap? bmp = CaptureWindow(hWnd))
            {
                if (bmp == null) return null;

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{fileNamePrefix}_0x{hWnd.ToInt64():X}_{timeStamp}.png";
                string fullPath = Path.Combine(desktopPath, fileName);

                bmp.Save(fullPath, ImageFormat.Png);
                return fullPath;
            }
        }

        /// <summary>
        /// ClientState üzerinde aktif olan seçili client'ın ekran görüntüsünü alıp Masaüstüne kaydeder.
        /// İşlem sonucunu durum bilgisi (Success), açıklama mesajı ve dosya yolu ile birlikte döndürür.
        /// </summary>
        public static (bool Success, string Message, string? FilePath) CaptureAndSaveSelectedClientToDesktop()
        {
            var selectedClient = ClientState.Instance.SelectedClient;

            if (selectedClient == null)
            {
                return (false, "Seçili bir istemci (Client) bulunamadı. Lütfen önce sol listeden bir client seçin.", null);
            }

            if (selectedClient.Handle == IntPtr.Zero || !Win32Native.IsWindow(selectedClient.Handle))
            {
                return (false, $"Seçili client ({selectedClient.Name}) için geçerli bir HWND penceresi bağlı değil.\nLütfen önce 'Seçili Client'a HWND Bağla' butonuna basarak pencere seçin.", null);
            }

            try
            {
                string safeClientName = string.Concat(selectedClient.Name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
                string prefix = $"Client_{selectedClient.Id}_{safeClientName}";
                string? savedPath = SaveWindowScreenshotToDesktop(selectedClient.Handle, prefix);

                if (!string.IsNullOrEmpty(savedPath))
                {
                    return (true, $"Ekran görüntüsü başarıyla alındı (Arka plan / Alttaki pencere dahil) ve Masaüstüne kaydedildi:\n{savedPath}", savedPath);
                }
                else
                {
                    return (false, "Pencere ekran görüntüsü alınırken bir hata oluştu veya görsel boş döndü.", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ekran görüntüsü kaydedilirken bir hata oluştu: {ex.Message}", null);
            }
        }
    }
}
