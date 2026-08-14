using Aether.Constants;
using Aether.Forms;
using Aether.Models;
using Aether.Native;
using Aether.States;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Aether.Helpers
{
    /// <summary>
    /// Ekran yakalama yöntemini belirleyen enum türü.
    /// </summary>
    public enum WindowCaptureMode
    {
        /// <summary>
        /// Otomatik mod: Önce en güvenli DXGI Desktop Duplication dener,
        /// başarısız/siyah olursa GDI Desktop Crop ve ardından PrintWindow dener.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// DXGI Desktop Duplication API (GPU / DWM Seviyesi):
        /// Oyuna veya HWND'ye asla doğrudan API isteği göndermez. Masaüstünün GPU üzerindeki
        /// tam karesini alıp GetWindowRect / ClientToScreen koordinatlarına göre keser (Crop).
        /// Anti-Cheat korumalarının siyah ekran vermesini %100 engeller.
        /// </summary>
        DxgiDesktopDuplication = 1,

        /// <summary>
        /// Masaüstü Ekran DC Kırpma (GDI Desktop Screen DC):
        /// Ekranın doğrudan sürücü DC'si üzerinden hedef pencerenin ekran koordinatlarını keser.
        /// </summary>
        DesktopCropGdi = 2,

        /// <summary>
        /// Standart Win32 PrintWindow:
        /// Pencere arkada veya simge durumundayken çizim yaptırır. (Bazı anti-cheat'ler siyah ekran verebilir)
        /// </summary>
        PrintWindow = 3
    }

    /// <summary>
    /// Pencere içerisinden belirli koordinat aralıklarında (baslangic_x, baslangic_y, bitis_x, bitis_y)
    /// Template Matching'e %100 uygun formatta ekran görüntüsü alan ve ekranda önizleme yapan yardımcı sınıf.
    /// 
    /// 🛡️ GÜVENLİ GELİŞTİRME & ANTI-CHEAT YAKLAŞIMI:
    /// 1. Sadece ekran görüntüsü almak (okuma işlemi) doğrudan oyuna müdahale (WriteProcessMemory, kod enjeksiyonu vb.)
    ///    etmediği için en düşük riskli işlemlerden biridir. Ancak anti-cheat pencerenin yakalanmasını teknik olarak engeller (siyah ekran verir).
    /// 2. DXGI (Desktop Duplication) Kullanımı: Oyun penceresinin görselini almak için oyuna veya HWND'ye doğrudan API isteği
    ///    atmak yerine, Windows'un ekran sürücüsü seviyesinde sunduğu DXGI Desktop Duplication API'sini kullanır.
    ///    Bu yöntem oyuna dokunmaz, masaüstünün GPU üzerindeki görüntüsünü okur.
    /// 3. Pencere Konumu ve Koordinat Eşleme: DXGI ile tüm ekranı çekip, oyun penceresinin masaüstündeki (GetWindowRect / ClientToScreen)
    ///    koordinatlarına denk gelen bölgeyi kesmek (Crop), oyun koruma yazılımlarının radarına takılmadan %100 görüntü alınmasını sağlar.
    /// </summary>
    public static class WindowRegionCaptureHelper
    {
        #region 1. Fonksiyon: İstenen Bölgenin Ekran Resmini Alma

        /// <summary>
        /// 1. FONKSİYON: Belirtilen HWND penceresinin iç alanından (baslangic_x, baslangic_y) ile (bitis_x, bitis_y)
        /// koordinatları arasındaki dikdörtgen/kare bölgenin ekran görüntüsünü çeker.
        /// DXGI Desktop Duplication, GDI Desktop Crop ve Win32 yöntemlerini akıllı hibrit sırayla kullanarak
        /// siyah ekran riskini ortadan kaldırır. Çıktı doğrudan OpenCvSharp / Template Matching ile kullanıma uygun
        /// (32bpp ARGB, 1:1 piksel) Bitmap formatındadır.
        /// </summary>
        /// <param name="hWnd">Yakalanacak pencerenin HWND adresi</param>
        /// <param name="startX">Başlangıç X piksel koordinatı (Sol - Client alanına göre)</param>
        /// <param name="startY">Başlangıç Y piksel koordinatı (Üst - Client alanına göre)</param>
        /// <param name="endX">Bitiş X piksel koordinatı (Sağ - Client alanına göre)</param>
        /// <param name="endY">Bitiş Y piksel koordinatı (Alt - Client alanına göre)</param>
        /// <param name="restoreIfIconic">Pencere simge durumundaysa arka planda uyandırsın mı? (Varsayılan: false, ses ve bildirimleri engeller)</param>
        /// <param name="captureMode">Kullanılacak yakalama stratejisi (Varsayılan: Auto / DXGI Destekli)</param>
        /// <returns>Template Matching'e hazır Bitmap nesnesi (başarısızsa null döner, çağıran Dispose etmelidir)</returns>
        public static Bitmap? CaptureRegion(
            IntPtr hWnd,
            int startX,
            int startY,
            int endX,
            int endY,
            bool restoreIfIconic = false,
            WindowCaptureMode captureMode = WindowCaptureMode.Auto)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return null;
            }

            // Koordinat sıralamasını doğrula (startX < endX ve startY < endY olmasını garanti et)
            int minX = Math.Min(startX, endX);
            int maxX = Math.Max(startX, endX);
            int minY = Math.Min(startY, endY);
            int maxY = Math.Max(startY, endY);

            int width = maxX - minX;
            int height = maxY - minY;

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            // Simge durumundaysa ve özellikle istendiyse uyandır
            if (restoreIfIconic && Win32Native.IsIconic(hWnd))
            {
                Win32Native.ShowWindow(hWnd, Win32Native.SW_SHOWNOACTIVATE);
                System.Threading.Thread.Sleep(50);
            }

            // 1. ÖNCELİK (Önerilen & Anti-Cheat Güvenli): DXGI Desktop Duplication + Pencere Koordinat Kırpma (Crop)
            if (captureMode == WindowCaptureMode.Auto || captureMode == WindowCaptureMode.DxgiDesktopDuplication)
            {
                Bitmap? dxgiBmp = CaptureRegionViaDxgi(hWnd, minX, minY, width, height);
                if (dxgiBmp != null && !IsBitmapBlank(dxgiBmp))
                {
                    return dxgiBmp;
                }
                dxgiBmp?.Dispose();

                if (captureMode == WindowCaptureMode.DxgiDesktopDuplication)
                {
                    return null;
                }
            }

            // 2. ÖNCELİK: Masaüstü Ekran GDI DC Kırpma (Driver seviyesi ekran koordinatından kesme)
            if (captureMode == WindowCaptureMode.Auto || captureMode == WindowCaptureMode.DesktopCropGdi)
            {
                Bitmap? gdiCropBmp = CaptureRegionViaDesktopCropGdi(hWnd, minX, minY, width, height);
                if (gdiCropBmp != null && !IsBitmapBlank(gdiCropBmp))
                {
                    return gdiCropBmp;
                }
                gdiCropBmp?.Dispose();

                if (captureMode == WindowCaptureMode.DesktopCropGdi)
                {
                    return null;
                }
            }

            // 3. ÖNCELİK: Klasik HWND PrintWindow ve iç alan kesme (Arka plan / Alt pencere yakalama)
            return CaptureRegionViaPrintWindow(hWnd, minX, minY, width, height, restoreIfIconic);
        }

        /// <summary>
        /// DXGI Desktop Duplication API'si ile tüm GPU masaüstünü okur ve pencerenin masaüstü koordinatlarına
        /// (ClientToScreen / GetWindowRect) denk gelen bölgesini keser (Crop).
        /// Oyuna dokunmadığı için Anti-Cheat sistemlerine takılmaz.
        /// </summary>
        public static Bitmap? CaptureRegionViaDxgi(IntPtr hWnd, int localX, int localY, int width, int height)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return null;
            }

            // Pencerenin ekran üzerindeki mutlak koordinatlarını hesapla
            Rectangle? screenTargetRect = CalculateScreenTargetRect(hWnd, localX, localY, width, height);
            if (!screenTargetRect.HasValue)
            {
                return null;
            }

            // DXGI Duplicator ile GPU üzerindeki masaüstü ekran bölgesini yakala
            return DxgiDesktopDuplicator.Instance.CaptureScreenRegion(screenTargetRect.Value);
        }

        /// <summary>
        /// Masaüstü ekran DC'si (CopyFromScreen) üzerinden pencere koordinatlarına denk gelen bölgeyi keser.
        /// </summary>
        public static Bitmap? CaptureRegionViaDesktopCropGdi(IntPtr hWnd, int localX, int localY, int width, int height)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return null;
            }

            Rectangle? screenTargetRect = CalculateScreenTargetRect(hWnd, localX, localY, width, height);
            if (!screenTargetRect.HasValue)
            {
                return null;
            }

            Rectangle rect = screenTargetRect.Value;
            try
            {
                Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(rect.X, rect.Y, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
                }
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Standart PrintWindow ile pencere iç alanını çekip belirtilen koordinatları kırpar.
        /// </summary>
        private static Bitmap? CaptureRegionViaPrintWindow(IntPtr hWnd, int minX, int minY, int width, int height, bool restoreIfIconic)
        {
            using (Bitmap? fullClientBmp = WindowCaptureHelper.CaptureWindow(hWnd, restoreIfIconic))
            {
                if (fullClientBmp == null)
                {
                    return null;
                }

                int clampedX = Math.Clamp(minX, 0, fullClientBmp.Width - 1);
                int clampedY = Math.Clamp(minY, 0, fullClientBmp.Height - 1);
                int safeWidth = Math.Min(width, fullClientBmp.Width - clampedX);
                int safeHeight = Math.Min(height, fullClientBmp.Height - clampedY);

                if (safeWidth <= 0 || safeHeight <= 0)
                {
                    return null;
                }

                Rectangle cropRectangle = new Rectangle(clampedX, clampedY, safeWidth, safeHeight);
                return fullClientBmp.Clone(cropRectangle, PixelFormat.Format32bppArgb);
            }
        }

        /// <summary>
        /// HWND penceresinin iç alanındaki yerel koordinatları masaüstü ekran koordinatlarına (Screen Coordinates) dönüştürür.
        /// </summary>
        public static Rectangle? CalculateScreenTargetRect(IntPtr hWnd, int localX, int localY, int width, int height)
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return null;
            }

            // İç alanın ekran başlangıç noktası
            Win32Native.POINT clientScreenPt = new Win32Native.POINT(0, 0);
            if (!Win32Native.ClientToScreen(hWnd, ref clientScreenPt))
            {
                // Fallback: GetWindowRect
                if (!Win32Native.GetWindowRect(hWnd, out Win32Native.RECT winRect))
                {
                    return null;
                }
                clientScreenPt.X = winRect.Left;
                clientScreenPt.Y = winRect.Top;
            }

            int screenX = clientScreenPt.X + localX;
            int screenY = clientScreenPt.Y + localY;

            return new Rectangle(screenX, screenY, width, height);
        }

        /// <summary>
        /// 1. FONKSİYON (Seçili Client İçin Kolay Çağrı):
        /// ClientState üzerindeki aktif seçili istemcinin penceresinden (startX, startY) - (endX, endY) bölgesini yakalar.
        /// </summary>
        /// <param name="startX">Başlangıç X koordinatı</param>
        /// <param name="startY">Başlangıç Y koordinatı</param>
        /// <param name="endX">Bitiş X koordinatı</param>
        /// <param name="endY">Bitiş Y koordinatı</param>
        /// <returns>Template Matching'e uygun Bitmap veya null</returns>
        public static Bitmap? CaptureRegion(int startX, int startY, int endX, int endY)
        {
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero)
            {
                return null;
            }

            return CaptureRegion(client.Handle, startX, startY, endX, endY);
        }

        /// <summary>
        /// Belirtilen Rectangle bölgesi üzerinden yakalama yapar.
        /// </summary>
        public static Bitmap? CaptureRegion(IntPtr hWnd, Rectangle region, bool restoreIfIconic = false, WindowCaptureMode captureMode = WindowCaptureMode.Auto)
        {
            return CaptureRegion(hWnd, region.Left, region.Top, region.Right, region.Bottom, restoreIfIconic, captureMode);
        }

        /// <summary>
        /// Tanımlı bir WindowRegion sabiti (Örn: RegionConstants.ChatBoxPosition) üzerinden HWND pencere bölgesini yakalar.
        /// </summary>
        /// <param name="hWnd">Hedef oyun penceresi HWND</param>
        /// <param name="region">Kırpılacak WindowRegion koordinat sabiti</param>
        /// <param name="restoreIfIconic">Pencere simge durumundaysa uyandırsın mı? (Varsayılan false)</param>
        /// <param name="captureMode">Yakalama modu (Varsayılan: Auto / DXGI)</param>
        /// <returns>Kırpılmış Bitmap görseli veya null</returns>
        public static Bitmap? CaptureRegion(IntPtr hWnd, WindowRegion region, bool restoreIfIconic = false, WindowCaptureMode captureMode = WindowCaptureMode.Auto)
        {
            return CaptureRegion(hWnd, region.StartX, region.StartY, region.EndX, region.EndY, restoreIfIconic, captureMode);
        }

        /// <summary>
        /// Aktif seçili istemci (SelectedClient) üzerinden tanımlı bir WindowRegion sabitini yakalar.
        /// </summary>
        /// <param name="region">Kırpılacak WindowRegion koordinat sabiti (Örn: RegionConstants.ChatBoxPosition)</param>
        /// <returns>Kırpılmış Bitmap görseli veya null</returns>
        public static Bitmap? CaptureRegion(WindowRegion region)
        {
            return CaptureRegion(region.StartX, region.StartY, region.EndX, region.EndY);
        }

        #endregion

        #region 2. Fonksiyon: Yakalanan Ekran Resmini Test İçin Ekranda Gösterme

        /// <summary>
        /// 2. FONKSİYON (TEST & ÖNİZLEME):
        /// Birinci fonksiyondan (CaptureRegion) dönen Bitmap görselini yeni ve modern bir form penceresinde ekranda gösterir.
        /// Pencere üzerinde görsel 1:1 piksel ölçeğinde incelenebilir, Masaüstüne PNG olarak kaydedilebilir veya
        /// 'Template Eşleme Testi' butonu ile anında şablon araması yapılabilir.
        /// </summary>
        /// <param name="image">Gösterilecek Bitmap görseli</param>
        /// <param name="title">Önizleme penceresinin başlığı</param>
        /// <param name="sourceHwnd">Opsiyonel: Kaynak pencere HWND'si (Canlı yenileme için)</param>
        /// <param name="sourceRegion">Opsiyonel: Kaynak koordinat bölgesi</param>
        /// <param name="clientName">Opsiyonel: İstemci adı</param>
        public static void ShowPreview(
            Bitmap image,
            string title = "Bölgesel Ekran Görüntüsü Önizleme",
            IntPtr sourceHwnd = default,
            Rectangle sourceRegion = default,
            string clientName = "")
        {
            if (image == null)
            {
                MessageBox.Show(
                    "Gösterilecek ekran görüntüsü bulunamadı (Görsel nesnesi null).",
                    "Önizleme Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            CapturePreviewForm previewForm = new CapturePreviewForm(
                image,
                title,
                sourceHwnd,
                sourceRegion,
                clientName);

            previewForm.Show();
        }

        /// <summary>
        /// TEST FONKSİYONU: Belirtilen pencerenin (HWND) başlık çubuğu ve kenarlıkları hariç TÜM İÇ ALANINI (Client Area)
        /// DXGI / Desktop Duplication veya akıllı hibrit mod ile çeker ve üzerinde fare ile istenen alanların seçilip
        /// koordinatlarının (baslangic_x, baslangic_y, bitis_x, bitis_y) doğrudan kopyalanabileceği interaktif test penceresini açar.
        /// </summary>
        /// <param name="hWnd">Pencere HWND adresi</param>
        /// <param name="clientName">İstemci adı</param>
        public static (bool Success, string Message) PreviewFullWindowWithSelection(IntPtr hWnd, string clientName = "")
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return (false, "Geçerli bir pencere (HWND) bağlı değil.");
            }

            // Pencere iç alan boyutlarını al
            if (!Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect) || clientRect.Width <= 0 || clientRect.Height <= 0)
            {
                if (!Win32Native.GetWindowRect(hWnd, out clientRect) || clientRect.Width <= 0 || clientRect.Height <= 0)
                {
                    return (false, "Pencere boyutları okunamadı.");
                }
            }

            // Tüm pencere iç alanını akıllı hibrit yakalama (DXGI Desktop Duplication -> GDI Crop -> PrintWindow) ile çek
            Bitmap? fullClientBmp = CaptureRegion(hWnd, 0, 0, clientRect.Width, clientRect.Height, restoreIfIconic: true, WindowCaptureMode.Auto);

            if (fullClientBmp == null)
            {
                // Son çare klasik CaptureWindow dene
                fullClientBmp = WindowCaptureHelper.CaptureWindow(hWnd);
            }

            if (fullClientBmp == null)
            {
                return (false, "Pencerenin iç alan görüntüsü alınamadı (Siyah ekran veya erişim engeli).");
            }

            CapturePreviewForm previewForm = new CapturePreviewForm(
                fullClientBmp,
                $"Pencere Koordinat Seçim Testi (DXGI/DWM) - {clientName} ({fullClientBmp.Width}x{fullClientBmp.Height} px)",
                hWnd,
                Rectangle.Empty,
                clientName);

            previewForm.Show();
            return (true, "Tam pencere test ekranı açıldı.");
        }

        /// <summary>
        /// Bir görselin tamamen siyah veya boş olup olmadığını kontrol eder.
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

        #endregion
    }
}
