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
        /// Belirtilen HWND (pencere tutacağı) üzerindeki pencerenin ekran görüntüsünü Bitmap olarak döndürür.
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

            if (!Win32Native.GetWindowRect(hWnd, out Win32Native.RECT rect))
            {
                return null;
            }

            int width = rect.Width;
            int height = rect.Height;

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            Bitmap? resultBitmap = null;

            // GDI Memory DC oluştur (Arka plandaki ve üst üste binmiş pencereleri sorunsuz çekebilmek için)
            IntPtr hdcScreen = Win32Native.GetDC(IntPtr.Zero);
            IntPtr hdcMem = Win32Native.CreateCompatibleDC(hdcScreen);
            IntPtr hBitmap = Win32Native.CreateCompatibleBitmap(hdcScreen, width, height);
            IntPtr hOldBmp = Win32Native.SelectObject(hdcMem, hBitmap);

            try
            {
                // 1. Öncelikli Yöntem: PrintWindow + PW_RENDERFULLCONTENT (0x02)
                // Windows DWM yönlendirme yüzeyini kullanarak pencere en altta/arkada olsa bile tam çizimini alır.
                bool printed = Win32Native.PrintWindow(hWnd, hdcMem, Win32Native.PW_RENDERFULLCONTENT);

                // 2. Yöntem: Başarısız olursa standart PrintWindow (0) dene
                if (!printed)
                {
                    printed = Win32Native.PrintWindow(hWnd, hdcMem, 0);
                }

                // 3. Yöntem: PrintWindow tamamen başarısız olursa pencerenin kendi DC'sinden BitBlt yap
                if (!printed)
                {
                    IntPtr hWindowDC = Win32Native.GetWindowDC(hWnd);
                    if (hWindowDC != IntPtr.Zero)
                    {
                        printed = Win32Native.BitBlt(hdcMem, 0, 0, width, height, hWindowDC, 0, 0, Win32Native.SRCCOPY);
                        Win32Native.ReleaseDC(hWnd, hWindowDC);
                    }
                }

                if (printed)
                {
                    using (Bitmap tempBmp = Image.FromHbitmap(hBitmap))
                    {
                        resultBitmap = new Bitmap(tempBmp);
                    }
                }
            }
            catch
            {
                resultBitmap = null;
            }
            finally
            {
                // GDI kaynaklarını serbest bırak
                Win32Native.SelectObject(hdcMem, hOldBmp);
                Win32Native.DeleteObject(hBitmap);
                Win32Native.DeleteDC(hdcMem);
                Win32Native.ReleaseDC(IntPtr.Zero, hdcScreen);
            }

            // Görüntü siyah/boş kalmışsa veya alınamadıysa ekrandan CopyFromScreen yedek yöntemini çalıştır
            if (resultBitmap == null || IsBitmapBlank(resultBitmap))
            {
                try
                {
                    Bitmap fallbackBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(fallbackBmp))
                    {
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
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
