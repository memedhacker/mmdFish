using System.Drawing;

namespace Aether.Constants
{
    /// <summary>
    /// Proje genelinde kullanılan sabit renk tanımlamaları.
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

        // Turuncu Renkler
        /// <summary> Turuncu (#FFB400 - RGB: 255, 180, 0) </summary>
        public static readonly Color Turuncu = Color.FromArgb(255, 180, 0);

        // Arka Plan ve Çizgi Renkleri
        /// <summary> Arka Plan Koyu (#18181B - RGB: 24, 24, 27) </summary>
        public static readonly Color ArkaPlanKoyu = Color.FromArgb(24, 24, 27);

        /// <summary> Arka Plan Açık (#1E1E23 - RGB: 30, 30, 35) </summary>
        public static readonly Color ArkaPlanAcik = Color.FromArgb(30, 30, 35);

        /// <summary> Çizgi Rengi (#3C3C41 - RGB: 60, 60, 65) </summary>
        public static readonly Color CizgiRengi = Color.FromArgb(60, 60, 65);

        #region Yapboz / Puzzle Renkleri (Puzzle Colors)

        /// <summary> Puzzle Red (#FF4326 - RGB: 255, 67, 38) </summary>
        public static readonly Color PuzzleRed = Color.FromArgb(255, 67, 38);
        public const string PuzzleRedHex = "#ff4326";

        /// <summary> Puzzle Green (#23E221 - RGB: 35, 226, 33) </summary>
        public static readonly Color PuzzleGreen = Color.FromArgb(35, 226, 33);
        public const string PuzzleGreenHex = "#23e221";

        /// <summary> Puzzle Yellow (#F0D431 - RGB: 240, 212, 49) </summary>
        public static readonly Color PuzzleYellow = Color.FromArgb(240, 212, 49);
        public const string PuzzleYellowHex = "#f0d431";

        /// <summary> Puzzle Orange (#FF8B18 - RGB: 255, 139, 24) </summary>
        public static readonly Color PuzzleOrange = Color.FromArgb(255, 139, 24);
        public const string PuzzleOrangeHex = "#ff8b18";

        /// <summary> Puzzle Cyan (#16FFFF - RGB: 22, 255, 255) </summary>
        public static readonly Color PuzzleCyan = Color.FromArgb(22, 255, 255);
        public const string PuzzleCyanHex = "#16ffff";

        /// <summary> Puzzle Blue (#0049FC - RGB: 0, 73, 252) </summary>
        public static readonly Color PuzzleBlue = Color.FromArgb(0, 73, 252);
        public const string PuzzleBlueHex = "#0049fc";

        /// <summary>
        /// Tüm puzzle renklerinin listesi.
        /// </summary>
        public static readonly Color[] AllPuzzleColors = new[]
        {
            PuzzleRed,
            PuzzleGreen,
            PuzzleYellow,
            PuzzleOrange,
            PuzzleCyan,
            PuzzleBlue
        };

        #endregion
    }
}
