using Aether.Forms;
using Aether.Models;
using Aether.Native;
using Aether.States;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aether.Helpers
{
    /// <summary>
    /// Pencere içerisinden belirli koordinat aralıklarında (baslangic_x, baslangic_y, bitis_x, bitis_y)
    /// Template Matching'e %100 uygun formatta ekran görüntüsü alan ve ekranda önizleme yapan yardımcı sınıf.
    /// </summary>
    public static class WindowRegionCaptureHelper
    {
        #region 1. Fonksiyon: İstenen Bölgenin Ekran Resmini Alma

        /// <summary>
        /// 1. FONKSİYON: Belirtilen HWND penceresinin iç alanından (baslangic_x, baslangic_y) ile (bitis_x, bitis_y)
        /// koordinatları arasındaki dikdörtgen/kare bölgenin ekran görüntüsünü çeker.
        /// Çıktı doğrudan OpenCvSharp / Template Matching ile kullanıma uygun (32bpp ARGB, 1:1 piksel) Bitmap formatındadır.
        /// </summary>
        /// <param name="hWnd">Yakalanacak pencerenin HWND adresi</param>
        /// <param name="startX">Başlangıç X piksel koordinatı (Sol)</param>
        /// <param name="startY">Başlangıç Y piksel koordinatı (Üst)</param>
        /// <param name="endX">Bitiş X piksel koordinatı (Sağ)</param>
        /// <param name="endY">Bitiş Y piksel koordinatı (Alt)</param>
        /// <param name="restoreIfIconic">Pencere simge durumundaysa arka planda uyandırsın mı?</param>
        /// <returns>Template Matching'e hazır Bitmap nesnesi (başarısızsa null döner, çağıran Dispose etmelidir)</returns>
        public static Bitmap? CaptureRegion(IntPtr hWnd, int startX, int startY, int endX, int endY, bool restoreIfIconic = true)
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

            // Pencerenin iç alanının (Client Area) tamamını arka planda yakala
            using (Bitmap? fullClientBmp = WindowCaptureHelper.CaptureWindow(hWnd, restoreIfIconic))
            {
                if (fullClientBmp == null)
                {
                    return null;
                }

                // Taşma (Out of Bounds) güvenliği: Koordinatları görsel boyutlarına sınırla
                int clampedX = Math.Clamp(minX, 0, fullClientBmp.Width - 1);
                int clampedY = Math.Clamp(minY, 0, fullClientBmp.Height - 1);
                int safeWidth = Math.Min(width, fullClientBmp.Width - clampedX);
                int safeHeight = Math.Min(height, fullClientBmp.Height - clampedY);

                if (safeWidth <= 0 || safeHeight <= 0)
                {
                    return null;
                }

                Rectangle cropRectangle = new Rectangle(clampedX, clampedY, safeWidth, safeHeight);

                // 1:1 piksel doğruluğunda ve kayıpsız 32bpp formatında bağımsız Bitmap kopyası oluştur
                return fullClientBmp.Clone(cropRectangle, PixelFormat.Format32bppArgb);
            }
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
        public static Bitmap? CaptureRegion(IntPtr hWnd, Rectangle region, bool restoreIfIconic = true)
        {
            return CaptureRegion(hWnd, region.Left, region.Top, region.Right, region.Bottom, restoreIfIconic);
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
                System.Windows.Forms.MessageBox.Show(
                    "Gösterilecek ekran görüntüsü bulunamadı (Görsel nesnesi null).",
                    "Önizleme Hatası",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
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
        /// TEST FONKSİYONU: Belirtilen pencerenin (HWND) başlık çubuğu ve kenarlıkları hariç TÜM İÇ ALANINI (Client Area) çeker
        /// ve üzerinde fare ile istenen alanların seçilip koordinatlarının (baslangic_x, baslangic_y, bitis_x, bitis_y)
        /// doğrudan kopyalanabileceği interaktif test penceresini açar.
        /// </summary>
        /// <param name="hWnd">Pencere HWND adresi</param>
        /// <param name="clientName">İstemci adı</param>
        public static (bool Success, string Message) PreviewFullWindowWithSelection(IntPtr hWnd, string clientName = "")
        {
            if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd))
            {
                return (false, "Geçerli bir pencere (HWND) bağlı değil.");
            }

            Bitmap? fullClientBmp = WindowCaptureHelper.CaptureWindow(hWnd);
            if (fullClientBmp == null)
            {
                return (false, "Pencerenin iç alan görüntüsü alınamadı.");
            }

            CapturePreviewForm previewForm = new CapturePreviewForm(
                fullClientBmp,
                $"Pencere Koordinat Seçim Testi - {clientName} ({fullClientBmp.Width}x{fullClientBmp.Height} px)",
                hWnd,
                Rectangle.Empty,
                clientName);

            previewForm.Show();
            return (true, "Tam pencere test ekranı açıldı.");
        }

        #endregion
    }
}
