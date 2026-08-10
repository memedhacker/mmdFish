using System.Drawing;

namespace Aether.Constants
{
    /// <summary>
    /// Proje genelinde kullanılan sabit renk tanımlamaları.
    /// 
    /// KULLANIM ÖRNEKLERİ:
    /// ------------------------------------------------------------------------
    /// 1. Sayfanın en üstüne namespace'i ekleyerek kullanım:
    ///    using Aether.Constants;
    ///    
    ///    cardPanel.RectColor = Colors.YesilKoyu;
    ///    button.BackColor = Colors.MaviKoyu;
    ///    label.ForeColor = Colors.PembeAcik;
    /// 
    /// 2. Direct (namespace eklemeden) kullanım:
    ///    this.BackColor = Aether.Constants.Colors.MaviAcik;
    /// ------------------------------------------------------------------------
    /// </summary>
    public static class Colors
    {
        // Mavi Renkler
        /// <summary> Mavi Koyu (#00B1FF - RGB: 0, 177, 255) </summary>
        public static readonly Color MaviKoyu = Color.FromArgb(0, 177, 255);

        /// <summary> Mavi Açık (#59BDFF - RGB: 89, 189, 255) </summary>
        public static readonly Color MaviAcik = Color.FromArgb(89, 189, 255);

        // Pembe Renkler
        /// <summary> Pembe Koyu (#F46788 - RGB: 244, 103, 136) </summary>
        public static readonly Color PembeKoyu = Color.FromArgb(244, 103, 136);

        /// <summary> Pembe Açık (#FF8BA4 - RGB: 255, 139, 164) </summary>
        public static readonly Color PembeAcik = Color.FromArgb(255, 139, 164);

        // Yeşil Renkler
        /// <summary> Yeşil Koyu (#63A847 - RGB: 99, 168, 71) </summary>
        public static readonly Color YesilKoyu = Color.FromArgb(99, 168, 71);

        /// <summary> Yeşil Açık (#87C16D - RGB: 135, 193, 109) </summary>
        public static readonly Color YesilAcik = Color.FromArgb(135, 193, 109);
    }
}
