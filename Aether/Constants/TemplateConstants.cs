using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace Aether.Constants
{
    /// <summary>
    /// Assets/templates klasöründeki tüm şablon görsel (template) dosyalarını strongly-typed sabitler,
    /// önbellekleme (caching) ve OpenCvSharp tabanlı Template Matching yöntemleri ile sunan merkezi sınıf.
    /// </summary>
    public static class TemplateConstants
    {
        #region 1. Şablon Dosya Yolu Sabitleri (Constants)

        /// <summary>
        /// AutoPass (otomatik geçiş / doğrulama) şablonları.
        /// </summary>
        public static class AutoPass
        {
            public const string MinikBalik = "autopass/minik_balik.png";
            public const string TatliSuKaridesi = "autopass/tatli_su_karidesi.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                MinikBalik,
                TatliSuKaridesi
            };
        }

        /// <summary>
        /// Balık isimleri ve tutulan nesnelere ait şablonlar.
        /// </summary>
        public static class FishNames
        {
            public const string AltinAnahtar = "fishnames/altin_anahtar.png";
            public const string AltinParcasi = "fishnames/altin_parcasi.png";
            public const string AltinSudak = "fishnames/altin_sudak.png";
            public const string AltinYuzuk = "fishnames/altin_yuzuk.png";
            public const string AynaliSazan = "fishnames/aynali_sazan.png";
            public const string BeyazSacBoyasi = "fishnames/beyaz_sac_boyasi.png";
            public const string BilgeKralinEldiveni = "fishnames/bilge_kralin_eldiveni.png";
            public const string BilgeKralinSembolu = "fishnames/bilge_kralin_sembolu.png";
            public const string BuyukSudak = "fishnames/buyuk_sudak.png";
            public const string DenizKiziAnahtari = "fishnames/deniz_kizi_anahtari.png";
            public const string DereAlabaligi = "fishnames/dere_alabaligi.png";
            public const string GokkusagiAlabaligi = "fishnames/gokkusagi_alabaligi.png";
            public const string GorunmezlikPelerini = "fishnames/gorunmezlik_pelerini.png";
            public const string GumusAnahtar = "fishnames/gumus_anahtar.png";
            public const string Hamsi = "fishnames/hamsi.png";
            public const string Kadife = "fishnames/kadife.png";
            public const string KahverengiSacBoyasi = "fishnames/kahverengi_sac_boyasi.png";
            public const string KirmiziSacBoyasi = "fishnames/kirmizi_sac_boyasi.png";
            public const string KralYengeci = "fishnames/kral_yengeci.png";
            public const string KurbagaBaligi = "fishnames/kurbaga_baligi.png";
            public const string Levrek = "fishnames/levrek.png";
            public const string LucyYuzugu = "fishnames/lucy_yuzugu.png";
            public const string Lufer = "fishnames/lufer.png";
            public const string NehirAlabaligi = "fishnames/nehir_alabaligi.png";
            public const string OtSazani = "fishnames/ot_sazani.png";
            public const string Palamut = "fishnames/palamut.png";
            public const string Ringa = "fishnames/ringa.png";
            public const string SacBoyasiTemizleyici = "fishnames/sac_boyasi_temizleyici.png";
            public const string SariSacBoyasi = "fishnames/sari_sac_boyasi.png";
            public const string Sazan = "fishnames/sazan.png";
            public const string SevimliBalik = "fishnames/sevimli_balik.png";
            public const string SiyahSacBoyasi = "fishnames/siyah_sac_boyasi.png";
            public const string SolucanTaktin = "fishnames/solucan_taktin.png";
            public const string Som = "fishnames/som.png";
            public const string Sudak = "fishnames/sudak.png";
            public const string Tekir = "fishnames/tekir.png";
            public const string Yabbie = "fishnames/yabbie.png";
            public const string YayinBaligi = "fishnames/yayin_baligi.png";
            public const string YilanBasiBaligi = "fishnames/yilan_basi_baligi.png";
            public const string Zargana = "fishnames/zargana.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                AltinAnahtar,
                AltinParcasi,
                AltinSudak,
                AltinYuzuk,
                AynaliSazan,
                BeyazSacBoyasi,
                BilgeKralinEldiveni,
                BilgeKralinSembolu,
                BuyukSudak,
                DenizKiziAnahtari,
                DereAlabaligi,
                GokkusagiAlabaligi,
                GorunmezlikPelerini,
                GumusAnahtar,
                Hamsi,
                Kadife,
                KahverengiSacBoyasi,
                KirmiziSacBoyasi,
                KralYengeci,
                KurbagaBaligi,
                Levrek,
                LucyYuzugu,
                Lufer,
                NehirAlabaligi,
                OtSazani,
                Palamut,
                Ringa,
                SacBoyasiTemizleyici,
                SariSacBoyasi,
                Sazan,
                SevimliBalik,
                SiyahSacBoyasi,
                SolucanTaktin,
                Som,
                Sudak,
                Tekir,
                Yabbie,
                YayinBaligi,
                YilanBasiBaligi,
                Zargana
            };
        }

        /// <summary>
        /// Durum ve bildirim şablonları (Waypoints).
        /// </summary>
        public static class Waypoints
        {
            public const string AltinTonBaligi1 = "waypoints/altin_ton_baligi1.png";
            public const string AltinTonBaligi2 = "waypoints/altin_ton_baligi2.png";
            public const string AltinTonBaligi3 = "waypoints/altin_ton_baligi3.png";
            public const string BiseyTakildi = "waypoints/bisey_takildi.png";
            public const string YakalananBalik = "waypoints/yakalanan_balik.png";
            public const string YemiKaybettin = "waypoints/yemi_kaybettin.png";

            public static readonly IReadOnlyList<string> All = new[]
            {
                AltinTonBaligi1,
                AltinTonBaligi2,
                AltinTonBaligi3,
                BiseyTakildi,
                YakalananBalik,
                YemiKaybettin
            };
        }

        /// <summary>
        /// Projedeki tüm şablonların listesi (48 adet).
        /// </summary>
        public static readonly IReadOnlyList<string> AllTemplates;

        static TemplateConstants()
        {
            var all = new List<string>();
            all.AddRange(AutoPass.All);
            all.AddRange(FishNames.All);
            all.AddRange(Waypoints.All);
            AllTemplates = all.AsReadOnly();
        }

        #endregion

        #region 2. Dosya Yolu Çözümleme (Path Resolution)

        /// <summary>
        /// Göreceli şablon yolunu (örn: "fishnames/sudak.png") disktteki mutlak (absolute) dosya yoluna dönüştürür.
        /// Hem derleme (bin/Debug/...) hem de geliştirme proje kök dizinini otomatik olarak kontrol eder.
        /// </summary>
        public static string? GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;

            string normalized = relativePath.Replace('/', Path.DirectorySeparatorChar)
                                            .Replace('\\', Path.DirectorySeparatorChar);

            // 1. Doğrudan veya BaseDirectory/Assets/templates/ kontrolü
            string p1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "templates", normalized);
            if (File.Exists(p1)) return p1;

            // 2. Geliştirme zamanı kaynak klasör kontrolü (../../../Assets/templates)
            string p2 = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "templates", normalized);
            if (File.Exists(p2)) return p2;

            // 3. BaseDirectory içinde doğrudan göreceli yol kontrolü
            string p3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, normalized);
            if (File.Exists(p3)) return p3;

            // 4. Doğrudan var olan mutlak yol kontrolü
            if (File.Exists(relativePath)) return relativePath;

            return null;
        }

        #endregion

        #region 3. OpenCvSharp Mat Önbellekleme (Cache & Memory Management)

        private static readonly ConcurrentDictionary<string, Mat> _matCache = new();
        private static readonly ConcurrentDictionary<string, Mat> _grayMatCache = new();
        private static readonly object _lockObj = new();

        /// <summary>
        /// Belirtilen şablonun OpenCvSharp <see cref="Mat"/> nesnesini önbellekten hızlıca döndürür.
        /// Önbellekte yoksa diskten yükleyip hafızaya alır.
        /// </summary>
        public static Mat? GetMat(string templateRelativePath, ImreadModes mode = ImreadModes.Color)
        {
            string cacheKey = $"{templateRelativePath}_{(int)mode}";
            if (_matCache.TryGetValue(cacheKey, out Mat? cachedMat) && cachedMat != null && !cachedMat.IsDisposed)
            {
                return cachedMat;
            }

            lock (_lockObj)
            {
                if (_matCache.TryGetValue(cacheKey, out cachedMat) && cachedMat != null && !cachedMat.IsDisposed)
                {
                    return cachedMat;
                }

                string? fullPath = GetFullPath(templateRelativePath);
                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    return null;
                }

                Mat loaded = Cv2.ImRead(fullPath, mode);
                if (loaded.Empty())
                {
                    loaded.Dispose();
                    return null;
                }

                _matCache[cacheKey] = loaded;
                return loaded;
            }
        }

        /// <summary>
        /// Şablonun Grayscale (Gri Tonlamalı) <see cref="Mat"/> kopyasını önbellekten döndürür.
        /// Gri tonlama karşılaştırmaları işlemci yükünü ciddi oranda azaltır.
        /// </summary>
        public static Mat? GetGrayMat(string templateRelativePath)
        {
            if (_grayMatCache.TryGetValue(templateRelativePath, out Mat? cachedGray) && cachedGray != null && !cachedGray.IsDisposed)
            {
                return cachedGray;
            }

            lock (_lockObj)
            {
                if (_grayMatCache.TryGetValue(templateRelativePath, out cachedGray) && cachedGray != null && !cachedGray.IsDisposed)
                {
                    return cachedGray;
                }

                Mat? colorMat = GetMat(templateRelativePath, ImreadModes.Color);
                if (colorMat == null || colorMat.Empty()) return null;

                Mat gray = new Mat();
                Cv2.CvtColor(colorMat, gray, ColorConversionCodes.BGR2GRAY);
                _grayMatCache[templateRelativePath] = gray;
                return gray;
            }
        }

        /// <summary>
        /// Tüm şablonları hafızaya önceden yükler (Preload).
        /// Bot döngüsünde ilk eşleşmedeki I/O gecikmelerini önlemek için başlatma aşamasında çağrılabilir.
        /// </summary>
        public static void PreloadAll()
        {
            foreach (var templatePath in AllTemplates)
            {
                GetMat(templatePath);
                GetGrayMat(templatePath);
            }
        }

        /// <summary>
        /// Önbellekteki tüm Mat nesnelerini dispose eder ve hafızayı serbest bırakır.
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObj)
            {
                foreach (var mat in _matCache.Values)
                {
                    if (mat != null && !mat.IsDisposed)
                    {
                        mat.Dispose();
                    }
                }
                _matCache.Clear();

                foreach (var mat in _grayMatCache.Values)
                {
                    if (mat != null && !mat.IsDisposed)
                    {
                        mat.Dispose();
                    }
                }
                _grayMatCache.Clear();
            }
        }

        #endregion

        #region 4. Template Matching (Şablon Eşleme) Yardımcı Metotları

        /// <summary>
        /// Kaynak görsel üzerinde (Mat) tek bir şablonu arar ve eşleşme sonucunu döndürür.
        /// </summary>
        /// <param name="sourceMat">Arama yapılacak ana görsel (Örn: Ekran görüntüsü)</param>
        /// <param name="templateRelativePath">TemplateConstants içerisindeki şablon yolu</param>
        /// <param name="threshold">Kabul edilebilir minimum benzerlik eşiği (Varsayılan: 0.85)</param>
        /// <param name="useGrayscale">Performans ve doğruluk için her iki görseli de gri tonda eşleştirsin mi? (Varsayılan: true)</param>
        /// <param name="mode">Eşleştirme algoritması (Varsayılan: CCoeffNormed)</param>
        public static TemplateMatchResult Match(
            Mat sourceMat,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            if (sourceMat == null || sourceMat.Empty())
                return TemplateMatchResult.Failed(templateRelativePath);

            Mat? templateMat = useGrayscale ? GetGrayMat(templateRelativePath) : GetMat(templateRelativePath);
            if (templateMat == null || templateMat.Empty())
                return TemplateMatchResult.Failed(templateRelativePath);

            if (sourceMat.Width < templateMat.Width || sourceMat.Height < templateMat.Height)
                return TemplateMatchResult.Failed(templateRelativePath);

            Mat? srcToUse = null;
            bool disposeSrc = false;

            try
            {
                if (useGrayscale && sourceMat.Channels() > 1)
                {
                    srcToUse = new Mat();
                    Cv2.CvtColor(sourceMat, srcToUse, ColorConversionCodes.BGR2GRAY);
                    disposeSrc = true;
                }
                else
                {
                    srcToUse = sourceMat;
                }

                using (Mat matchResult = new Mat())
                {
                    Cv2.MatchTemplate(srcToUse, templateMat, matchResult, mode);
                    Cv2.MinMaxLoc(matchResult, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);

                    bool isNormalizedSqDiff = (mode == TemplateMatchModes.SqDiff || mode == TemplateMatchModes.SqDiffNormed);
                    double score = isNormalizedSqDiff ? (1.0 - minVal) : maxVal;
                    Point bestLoc = isNormalizedSqDiff ? minLoc : maxLoc;

                    bool isSuccess = isNormalizedSqDiff ? (minVal <= (1.0 - threshold)) : (maxVal >= threshold);

                    if (isSuccess)
                    {
                        return new TemplateMatchResult
                        {
                            IsSuccess = true,
                            TemplatePath = templateRelativePath,
                            TemplateName = Path.GetFileNameWithoutExtension(templateRelativePath),
                            Confidence = score,
                            Location = new System.Drawing.Point(bestLoc.X, bestLoc.Y),
                            Bounds = new Rectangle(bestLoc.X, bestLoc.Y, templateMat.Width, templateMat.Height)
                        };
                    }

                    return new TemplateMatchResult
                    {
                        IsSuccess = false,
                        TemplatePath = templateRelativePath,
                        TemplateName = Path.GetFileNameWithoutExtension(templateRelativePath),
                        Confidence = score,
                        Location = new System.Drawing.Point(bestLoc.X, bestLoc.Y),
                        Bounds = Rectangle.Empty
                    };
                }
            }
            finally
            {
                if (disposeSrc && srcToUse != null)
                {
                    srcToUse.Dispose();
                }
            }
        }

        /// <summary>
        /// GDI+ Bitmap formatındaki kaynak görsel üzerinde şablon eşleştirmesi yapar.
        /// </summary>
        public static TemplateMatchResult Match(
            Bitmap sourceBitmap,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true,
            TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)
        {
            if (sourceBitmap == null) return TemplateMatchResult.Failed(templateRelativePath);

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return Match(srcMat, templateRelativePath, threshold, useGrayscale, mode);
            }
        }

        /// <summary>
        /// Kaynak görselde belirtilen şablonun bulunup bulunmadığını hızlıca doğrular (bool döner).
        /// </summary>
        public static bool Contains(
            Mat sourceMat,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            var res = Match(sourceMat, templateRelativePath, threshold, useGrayscale);
            return res.IsSuccess;
        }

        /// <summary>
        /// Bitmap formatındaki kaynak görselde belirtilen şablonun bulunup bulunmadığını hızlıca doğrular.
        /// </summary>
        public static bool Contains(
            Bitmap sourceBitmap,
            string templateRelativePath,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            var res = Match(sourceBitmap, templateRelativePath, threshold, useGrayscale);
            return res.IsSuccess;
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak görselde en yüksek benzerlik puanına sahip olan en iyi eşleşmeyi bulur.
        /// Balık adı tespitinde veya durum kontrolünde çok kullanışlıdır.
        /// </summary>
        public static TemplateMatchResult? FindBestMatch(
            Mat sourceMat,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceMat == null || sourceMat.Empty() || candidateTemplatePaths == null)
                return null;

            TemplateMatchResult? bestResult = null;
            double highestScore = -1.0;

            foreach (var templatePath in candidateTemplatePaths)
            {
                var result = Match(sourceMat, templatePath, minThreshold, useGrayscale);
                if (result.IsSuccess && result.Confidence > highestScore)
                {
                    highestScore = result.Confidence;
                    bestResult = result;
                }
            }

            return bestResult;
        }

        /// <summary>
        /// Verilen şablon listesi arasında kaynak Bitmap üzerinde en iyi eşleşmeyi bulur.
        /// </summary>
        public static TemplateMatchResult? FindBestMatch(
            Bitmap sourceBitmap,
            IEnumerable<string> candidateTemplatePaths,
            double minThreshold = 0.80,
            bool useGrayscale = true)
        {
            if (sourceBitmap == null) return null;

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return FindBestMatch(srcMat, candidateTemplatePaths, minThreshold, useGrayscale);
            }
        }

        /// <summary>
        /// Verilen şablon listesindeki eşik değerini geçen TÜM eşleşmeleri liste olarak döndürür.
        /// </summary>
        public static List<TemplateMatchResult> FindAllMatches(
            Mat sourceMat,
            IEnumerable<string> candidateTemplatePaths,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            var results = new List<TemplateMatchResult>();
            if (sourceMat == null || sourceMat.Empty() || candidateTemplatePaths == null)
                return results;

            foreach (var templatePath in candidateTemplatePaths)
            {
                var result = Match(sourceMat, templatePath, threshold, useGrayscale);
                if (result.IsSuccess)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Bitmap formatındaki kaynak görselde eşik değerini geçen tüm eşleşmeleri döndürür.
        /// </summary>
        public static List<TemplateMatchResult> FindAllMatches(
            Bitmap sourceBitmap,
            IEnumerable<string> candidateTemplatePaths,
            double threshold = 0.85,
            bool useGrayscale = true)
        {
            if (sourceBitmap == null) return new List<TemplateMatchResult>();

            using (Mat srcMat = BitmapToMat(sourceBitmap))
            {
                return FindAllMatches(srcMat, candidateTemplatePaths, threshold, useGrayscale);
            }
        }

        #endregion

        #region 5. Bitmap <-> Mat Dönüştürücü (Converter)

        /// <summary>
        /// Standart System.Drawing.Bitmap nesnesini OpenCvSharp.Mat nesnesine bellek üzerinden dönüştürür.
        /// </summary>
        public static Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));

            BitmapData? bmpData = null;
            try
            {
                PixelFormat format = bitmap.PixelFormat;
                int channels = 3;
                MatType matType = MatType.CV_8UC3;

                if (format == PixelFormat.Format24bppRgb)
                {
                    channels = 3;
                    matType = MatType.CV_8UC3;
                }
                else if (format == PixelFormat.Format32bppArgb || format == PixelFormat.Format32bppPArgb || format == PixelFormat.Format32bppRgb)
                {
                    channels = 4;
                    matType = MatType.CV_8UC4;
                }
                else if (format == PixelFormat.Format8bppIndexed)
                {
                    channels = 1;
                    matType = MatType.CV_8UC1;
                }
                else
                {
                    // Diğer formatlar için 24bppRGB'ye dönüştürerek oku
                    using (Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb))
                    {
                        using (Graphics g = Graphics.FromImage(clone))
                        {
                            g.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
                        }
                        return BitmapToMat(clone);
                    }
                }

                bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    format);

                Mat mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, matType, bmpData.Scan0, bmpData.Stride);
                Mat result = mat.Clone(); // Veriyi bağımsız kopyala
                mat.Dispose();

                // Eğer 4 kanallıysa (BGRA), standart BGR'ye dönüştürebiliriz
                if (channels == 4)
                {
                    Mat bgr = new Mat();
                    Cv2.CvtColor(result, bgr, ColorConversionCodes.BGRA2BGR);
                    result.Dispose();
                    return bgr;
                }

                return result;
            }
            finally
            {
                if (bmpData != null)
                {
                    bitmap.UnlockBits(bmpData);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Şablon eşleme (Template Matching) sonucunu ifade eden model sınıfı.
    /// </summary>
    public class TemplateMatchResult
    {
        /// <summary> Eşleşme başarılı ve belirlenen eşik değerinin üzerinde mi? </summary>
        public bool IsSuccess { get; set; }

        /// <summary> Eşleşen şablon dosyasının göreceli yolu (Örn: "fishnames/sudak.png") </summary>
        public string TemplatePath { get; set; } = string.Empty;

        /// <summary> Şablonun dosya adı (Uzantısız, Örn: "sudak") </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary> Benzerlik katsayısı / Güven oranı (0.00 ile 1.00 arası, örn: 0.96) </summary>
        public double Confidence { get; set; }

        /// <summary> Eşleşmenin kaynak görsel üzerindeki sol-üst (X, Y) piksel koordinatı </summary>
        public System.Drawing.Point Location { get; set; }

        /// <summary> Eşleşen bölgenin dikdörtgen sınırları (X, Y, Width, Height) </summary>
        public Rectangle Bounds { get; set; }

        /// <summary> Eşleşen bölgenin merkez piksel koordinatı </summary>
        public System.Drawing.Point CenterPoint => new(
            Location.X + (Bounds.Width / 2),
            Location.Y + (Bounds.Height / 2));

        public static TemplateMatchResult Failed(string templatePath) => new()
        {
            IsSuccess = false,
            TemplatePath = templatePath,
            TemplateName = Path.GetFileNameWithoutExtension(templatePath),
            Confidence = 0,
            Location = System.Drawing.Point.Empty,
            Bounds = Rectangle.Empty
        };

        public override string ToString() =>
            IsSuccess
                ? $"[{TemplateName}] Eşleşti! Benzerlik: %{Confidence * 100:F1}, Konum: ({Location.X}, {Location.Y}), Boyut: {Bounds.Width}x{Bounds.Height}"
                : $"[{TemplateName}] Eşleşmedi. En Yüksek Puan: %{Confidence * 100:F1}";
    }
}
