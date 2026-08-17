# 🐟 Balık Template Renk Analizi ve Ayırt Etme Rehberi

## 1. Problem Tanımı

Oyun envanterindeki balık ikonlarının üç farklı durumu vardır:

| Durum | Dosya Prefix | Görsel Fark |
|-------|-------------|-------------|
| **Normal (Canlı)** | `Levrek.png` | Doğal, canlı renkler |
| **Ölü (Dead)** | `Ölü_Levrek.png` | Soluk, gri/desature tonlar |
| **Izgara (Grilled)** | `Izgara_Levrek.png` | Kahverengi/yanık tonlar, koyu |

**Temel Sorun:** Bu üç durumun **şekilleri (silhouette/contour) neredeyse aynıdır**. OpenCV `Cv2.MatchTemplate()` fonksiyonu `useGrayscale: true` modunda kullanıldığında şekil benzerliğine odaklanır ve **Ölü balığı Normal ile, Izgara'yı Normal ile karıştırır.** `useGrayscale: false` bile tam çözüm sağlamaz çünkü CCoeffNormed korelasyonu çok benzerse hâlâ yüksek skor verir.

---

## 2. Renk Analizi Sonuçları (24 Balık Türü × 3 Durum)

### 2.1 Genel Ortalamalar

| Metrik | 🟢 Normal | 💀 Ölü | 🔥 Izgara |
|--------|----------|--------|----------|
| **RGB Ortalama** | R=125.6, G=113.2, B=92.5 | R=106.4, G=100.5, B=93.4 | R=100.5, G=77.8, B=54.3 |
| **HSV Ortalama** | H=91°, S=33%, V=51% | H=82°, S=15.5%, V=42% | H=42°, S=45.5%, V=39.5% |
| **R/G Oranı** | 1.12 ± 0.17 | 1.06 ± 0.07 | 1.31 ± 0.29 |
| **R/B Oranı** | 1.45 | 1.15 | 1.95 |
| **Saturation Farkı** | 40.0 | 14.8 | 46.2 |
| **Ortalama Parlaklık** | 110.4 | 100.1 | 77.5 |

### 2.2 Balık Bazında Detaylı Veriler (Common)

```
Balık Adı              | Durum   | RGB                | HSV                | R/G  | Sat
-----------------------|---------|--------------------|--------------------|------|----
Büyük_Sudak_Balığı     | Normal  | (151,151,109)      | (66°,30%,60%)      | 1.00 | 42
                       | Ölü     | (124,119,106)      | (46°,16%,49%)      | 1.05 | 18
                       | Izgara  | (91,75,54)          | (33°,40%,36%)      | 1.22 | 37
-----------------------|---------|--------------------|--------------------|------|----
Levrek                 | Normal  | (78,76,63)          | (112°,21%,32%)     | 1.03 | 15
                       | Ölü     | (70,68,64)          | (81°,10%,28%)      | 1.04 | 7
                       | Izgara  | (86,70,52)          | (38°,39%,34%)      | 1.23 | 34
-----------------------|---------|--------------------|--------------------|------|----
Hamsi                  | Normal  | (150,126,93)        | (35°,38%,59%)      | 1.20 | 58
                       | Ölü     | (113,91,83)         | (28°,26%,44%)      | 1.23 | 29
                       | Izgara  | (93,76,55)          | (32°,40%,37%)      | 1.22 | 39
-----------------------|---------|--------------------|--------------------|------|----
Lüfer_Balığı           | Normal  | (109,116,121)       | (177°,18%,49%)     | 0.94 | 13
                       | Ölü     | (111,113,107)       | (90°,7%,44%)       | 0.98 | 6
                       | Izgara  | (96,85,66)          | (84°,27%,38%)      | 1.14 | 30
-----------------------|---------|--------------------|--------------------|------|----
Yayın_Balığı           | Normal  | (121,119,131)       | (217°,18%,53%)     | 1.01 | 12
                       | Ölü     | (113,114,107)       | (64°,8%,45%)       | 1.00 | 6
                       | Izgara  | (99,83,54)          | (38°,47%,39%)      | 1.20 | 45
```

### 2.3 Balık Bazında Detaylı Veriler (Rare)

```
Balık Adı              | Durum   | RGB                | HSV                | R/G  | Sat
-----------------------|---------|--------------------|--------------------|------|----
Altın_Sudak_Balığı     | Normal  | (173,141,67)        | (43°,66%,68%)      | 1.22 | 105
                       | Ölü     | (136,126,102)       | (45°,27%,53%)      | 1.08 | 33
                       | Izgara  | (92,71,44)          | (43°,60%,36%)      | 1.29 | 48
-----------------------|---------|--------------------|--------------------|------|----
Kral_Yengeci           | Normal  | (143,87,51)         | (30°,66%,56%)      | 1.64 | 92
                       | Ölü     | (101,86,77)         | (48°,26%,40%)      | 1.17 | 25
                       | Izgara  | (133,56,33)         | (14°,77%,52%)      | 2.39 | 101
-----------------------|---------|--------------------|--------------------|------|----
Kurbağa_Balığı         | Normal  | (119,118,146)       | (238°,25%,58%)     | 1.01 | 28
                       | Ölü     | (113,113,121)       | (240°,11%,48%)     | 1.00 | 8
                       | Izgara  | (99,85,85)          | (185°,21%,40%)     | 1.17 | 14
```

---

## 3. Temel Ayırt Edici Özellikler

### 3.1 Normal vs Ölü

> [!IMPORTANT]
> **En güçlü ayırt edici: SATURATION (Doygunluk)**
> - Normal ortalama: **S = 33%**
> - Ölü ortalama: **S = 15.5%**
> - Ölü balıklar **her zaman daha soluk/gri** görünür

| Özellik | Normal | Ölü | Fark |
|---------|--------|-----|------|
| **Saturation (HSV-S)** | 33.0% | 15.5% | -17.5 |
| **Saturation Diff (max-min RGB)** | 40.0 | 14.8 | -25.2 |
| **R/G Oranı** | 1.12 | 1.06 | Ölü daha nötr |
| **Parlaklık** | 110.4 | 100.1 | Ölü biraz daha koyu |

**Sonuç:** Ölü balıklar RGB kanalları arasında çok az fark gösterir (R≈G≈B), yani gri-ye yakındır. Normal balıklar daha doygun/canlı renklere sahiptir.

### 3.2 Normal vs Izgara

> [!IMPORTANT]
> **En güçlü ayırt ediciler: HUE (Ton) + PARLAKLIK + R/B ORANI**
> - Normal ortalama: **H = 91°** (yeşilimsi-sarımsı)
> - Izgara ortalama: **H = 42°** (turuncu-kahverengi)
> - Izgara **çok daha koyu** (brightness: 77.5 vs 110.4)

| Özellik | Normal | Izgara | Fark |
|---------|--------|--------|------|
| **Hue (HSV-H)** | 91° | 42° | -49° (Izgara daha turuncu) |
| **R/B Oranı** | 1.45 | 1.95 | Izgara'da kırmızı baskın |
| **Parlaklık** | 110.4 | 77.5 | -32.9 (Izgara çok koyu) |
| **Saturation** | 33% | 45.5% | Izgara daha doygun |

### 3.3 Ölü vs Izgara

| Özellik | Ölü | Izgara | Fark |
|---------|-----|--------|------|
| **Saturation** | 15.5% | 45.5% | +30 (Izgara daha doygun) |
| **R/G Oranı** | 1.06 | 1.31 | Izgara'da kırmızı baskın |
| **R/B Oranı** | 1.15 | 1.95 | Izgara'da mavi bastırılmış |
| **Parlaklık** | 100.1 | 77.5 | Izgara daha koyu |

---

## 4. Önerilen Algoritmik Yaklaşımlar

### Yaklaşım 1: İki Aşamalı Template Matching + Renk Doğrulama (Önerilen)

```
1. ADIM: Template Matching ile konum bul
   - Envanter bölgesinde FindAllMatches çağır
   - Normal + Ölü + Izgara tüm templatelerle eşleştir
   - useGrayscale: false kullan (renk farkını koru)
   
2. ADIM: Eşleşen bölgenin renk istatistiklerini çıkar
   - Bulunan her eşleşmenin Bounds bölgesini kırp
   - Bu ROI (Region of Interest) üzerinde HSV analizi yap
   - Saturation ortalaması, R/G oranı, parlaklık hesapla
   
3. ADIM: Karar ağacı ile durumu belirle
   if (saturation < 20% VE satDiff < 20):
       → ÖLÜ BALIK
   elif (hue < 55° VE R/B > 1.6 VE brightness < 90):
       → IZGARA BALIK
   else:
       → NORMAL BALIK
```

### Yaklaşım 2: Sadece Template Matching (Daha Basit, Daha Riskli)

```
- Her üç durum için ayrı template listeleri oluştur
- useGrayscale: false modunda CCoeffNormed ile eşleştir
- Her template setini ayrı ayrı tara
- En yüksek skoru alan set kazanır
- Eşik değerini 0.88+ yap (benzer şekiller ayrışsın)

RISK: Bazı balıklarda Normal-Ölü arası confidence farkı < 0.05 olabilir
```

### Yaklaşım 3: HSV Histogram Karşılaştırma (En Doğru, En Yavaş)

```
1. Her template'in HSV histogramını önceden hesapla ve cache'le
2. Envanter slot'undaki ikonu kırp
3. HSV histogramını hesapla
4. Cv2.CompareHist ile tüm template'lerle karşılaştır
5. En yüksek korelasyonu veren template'i seç

Avantaj: Şekil yerine renk dağılımına odaklanır
Dezavantaj: Performans (histogram hesaplama her frame'de)
```

---

## 5. C# Kod Önerileri

### 5.1 ROI Renk Analizi Yardımcı Metodu

```csharp
/// <summary>
/// Verilen Mat bölgesinin ortalama HSV değerlerini ve renk oranlarını hesaplar.
/// Ölü/Normal/Izgara ayrımı için kullanılır.
/// </summary>
public static (double Hue, double Saturation, double Value, double RgRatio, double RbRatio, double SatDiff) 
    AnalyzeRoiColor(Mat roi)
{
    if (roi == null || roi.Empty()) 
        return (0, 0, 0, 1, 1, 0);

    // BGR -> HSV dönüşümü
    using Mat hsv = new Mat();
    Cv2.CvtColor(roi, hsv, ColorConversionCodes.BGR2HSV);

    // Şeffaf/siyah pikselleri filtrele (V > 20)
    Scalar meanBgr = Cv2.Mean(roi);
    Scalar meanHsv = Cv2.Mean(hsv);

    double r = meanBgr[2]; // OpenCV'de BGR sırası
    double g = meanBgr[1];
    double b = meanBgr[0];

    double rgRatio = g > 1 ? r / g : r;
    double rbRatio = b > 1 ? r / b : r;
    double satDiff = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));

    return (
        Hue: meanHsv[0],          // 0-180 (OpenCV'de H yarım ölçekli)
        Saturation: meanHsv[1],    // 0-255
        Value: meanHsv[2],         // 0-255
        RgRatio: rgRatio,
        RbRatio: rbRatio,
        SatDiff: satDiff
    );
}
```

### 5.2 Balık Durumu Karar Mekanizması

```csharp
public enum FishState { Normal, Dead, Grilled, Unknown }

/// <summary>
/// ROI renk istatistiklerine göre balığın durumunu belirler.
/// </summary>
public static FishState ClassifyFishState(Mat fishRoi)
{
    var (hue, sat, val, rgRatio, rbRatio, satDiff) = AnalyzeRoiColor(fishRoi);

    // OpenCV HSV: H=0-180, S=0-255, V=0-255
    double satPercent = (sat / 255.0) * 100.0;
    double valPercent = (val / 255.0) * 100.0;
    double hueScaled = hue * 2; // 0-360 ölçeğe çevir

    // ÖLÜ: Düşük doygunluk, RGB kanalları birbirine yakın
    if (satPercent < 20 && satDiff < 20)
        return FishState.Dead;

    // IZGARA: Düşük hue (turuncu/kahve), yüksek R/B oranı, düşük parlaklık
    if (hueScaled < 55 && rbRatio > 1.6 && valPercent < 45)
        return FishState.Grilled;

    // Varsayılan: NORMAL
    return FishState.Normal;
}
```

### 5.3 Mevcut Sisteme Entegrasyon

```csharp
// Envanter taramasında kullanım örneği:
var allFishTemplates = new List<string>();
allFishTemplates.AddRange(TemplateConstants.FishIconTemplates.CommonFishes.All);
allFishTemplates.AddRange(TemplateConstants.FishIconTemplates.RareFishes.All);
// Ölü ve Izgara'yı DA dahil et (şekil eşleşmesi için)
allFishTemplates.AddRange(TemplateConstants.FishIconTemplates.DeadFishes.All);
allFishTemplates.AddRange(TemplateConstants.FishIconTemplates.GrilledFishes.All);

var allMatches = TemplateConstants.FindAllMatches(
    inventoryBmp, 
    allFishTemplates, 
    threshold: 0.80, 
    useGrayscale: false  // RENK ZORUNLU!
);

foreach (var match in allMatches)
{
    // Eşleşen bölgeyi kırp ve renk analizi yap
    using Mat roi = new Mat(inventoryMat, 
        new Rect(match.Location.X, match.Location.Y, match.Bounds.Width, match.Bounds.Height));
    
    FishState state = ClassifyFishState(roi);
    
    // Template adından da kontrol et (çift doğrulama)
    bool templateSaysDead = match.TemplateName.StartsWith("Ölü_");
    bool templateSaysGrilled = match.TemplateName.StartsWith("Izgara_");
    
    // Renk analizi ile template adı uyuşmazsa → renk analizine güven
    // Çünkü template matching şekil benzerliğinden yanılabilir
}
```

---

## 6. Kritik Kurallar ve Uyarılar

> [!CAUTION]
> Template matching'de **asla** `useGrayscale: true` kullanma! Grayscale'e dönüştürdüğünde tüm renk bilgisi kaybolur ve Normal/Ölü/Izgara ayrımı imkansız hale gelir.

> [!WARNING]
> Bazı balık türleri (örn: Hamsi, Sazan) Normal ve Ölü halleri arasında düşük renk farkına sahiptir. Bu türlerde eşik değerlerini daha hassas tut.

> [!TIP]
> En güvenilir yaklaşım: **Template matching ile konumu bul → Renk analizi ile durumu doğrula** (iki aşamalı). Sadece template matching'e güvenme.

---

## 7. AI Prompt Örnekleri

Aşağıdaki promptları Gemini, Claude veya diğer AI modellerine vererek balık durumu ayırt etme kodu yazdırabilirsiniz:

---

### Prompt 1: Temel Renk Analizi Fonksiyonu

```
C# ve OpenCvSharp4 kullanıyorum. Elimde balık ikonlarının 3 farklı durumu var:
- Normal (canlı balık): Doğal, doygun renkler. Ortalama HSV: H=91°, S=33%, V=51%
- Ölü (dead): Soluk, gri tonlu. Ortalama HSV: H=82°, S=15.5%, V=42%
- Izgara (grilled): Kahverengi/yanık. Ortalama HSV: H=42°, S=45.5%, V=39.5%

Bu üç durumun şekilleri (silhouette) neredeyse aynı. Template matching ile konumlarını bulabiliyorum ama hangi durumda olduklarını ayırt edemiyorum.

Lütfen bir `FishState ClassifyFishState(Mat fishRoi)` metodu yaz:
1. Mat olarak verilen balık bölgesinin (ROI) ortalama HSV değerlerini hesapla
2. Şeffaf/siyah arka plan piksellerini filtrele (V < 20 olanları atla)
3. Saturation, R/G oranı ve parlaklık değerlerine göre Normal/Ölü/Izgara ayırt et
4. Eşik değerleri: Ölü → S<20%, SatDiff<20 | Izgara → H<55°, R/B>1.6, V<45%

OpenCV'de HSV değerleri: H=0-180, S=0-255, V=0-255 (standart ölçek değil).
```

---

### Prompt 2: İki Aşamalı Envanter Tarama Sistemi

```
C# (.NET 10), OpenCvSharp4 ve WinForms kullanıyorum. Bir oyunda envanter ekranındaki
balık ikonlarını tespit edip işlem yapan bir bot yazıyorum.

## Mevcut Sistem
- `TemplateConstants.FindAllMatches(Bitmap source, IEnumerable<string> templates, 
  threshold, useGrayscale)` → `List<TemplateMatchResult>` döner
- TemplateMatchResult: IsSuccess, TemplatePath, TemplateName, Confidence, Location, Bounds
- Template listeleri:
  - TemplateConstants.FishIconTemplates.CommonFishes.All (15 normal balık)
  - TemplateConstants.FishIconTemplates.RareFishes.All (9 nadir balık)
  - TemplateConstants.FishIconTemplates.DeadFishes.All (24 ölü balık, "Ölü_" prefix)
  - TemplateConstants.FishIconTemplates.GrilledFishes.All (24 ızgara, "Izgara_" prefix)

## Problem
Normal, Ölü ve Izgara balıkların şekilleri aynı, sadece renkleri farklı.
Template matching bazen Normal balığı Ölü olarak, Ölü'yü Normal olarak eşleştiriyor.

## Renk Farkları (24 balık türünün ortalaması)
- Normal: RGB=(125.6, 113.2, 92.5), Saturation=33%, Brightness=110.4
- Ölü:    RGB=(106.4, 100.5, 93.4), Saturation=15.5%, Brightness=100.1  (SOLUK/GRİ)
- Izgara: RGB=(100.5, 77.8, 54.3),  Saturation=45.5%, Brightness=77.5   (KAHVERENGİ/KOYU)

## İstenen
İki aşamalı bir envanter tarama sistemi yaz:
1. Tüm template'leri birden tara (useGrayscale: false)
2. Her bulunan eşleşme için ROI bölgesinin renk analizini yap
3. Renk analizine göre kesin durumu belirle (Normal/Ölü/Izgara)
4. Sonucu `(TemplateMatchResult match, FishState state)` tuple listesi olarak döndür

Not: Performans önemli, envanter taraması frame başına yapılacak.
```

---

### Prompt 3: Histogram Tabanlı Karşılaştırma

```
C# ve OpenCvSharp4 ile çalışıyorum. Şekilleri aynı ama renkleri farklı olan küçük
ikon resimleri (~32x32px) arasında ayırt etmem gerekiyor.

3 durum var:
- Normal: Doygun, doğal renkler (S ortalama 33%)  
- Dead: Soluk, gri tonlar (S ortalama 15.5%, R≈G≈B)
- Grilled: Koyu kahverengi (H~42°, R/B>1.95, düşük parlaklık)

Bana bir HSV histogram karşılaştırma sistemi yaz:
1. Verilen bir Bitmap'i HSV'ye çevir
2. H ve S kanallarının 2D histogramını hesapla (H: 30 bin, S: 32 bin)
3. Önceden hesaplanmış template histogramlarıyla Cv2.CompareHist ile karşılaştır
4. CORREL veya BHATTACHARYYA yöntemi kullan
5. Template histogramlarını başlangıçta hesaplayıp cache'leyen bir sistem de ekle

Fonksiyon imzası:
`(string bestTemplateName, double similarity) FindBestHistogramMatch(
    Mat roi, Dictionary<string, Mat> templateHistograms)`
```

---

### Prompt 4: Sadece Ölü/Canlı Ayrımı (Basit)

```
C# ve OpenCvSharp4 kullanıyorum. Bir Mat (BGR) olarak verilmiş küçük bir balık
ikonunun "canlı mı yoksa ölü mü" olduğunu belirlemem gerekiyor.

Veriler:
- Canlı balık: Ortalama Saturation = 33%, RGB kanalları arası fark (max-min) = 40
- Ölü balık: Ortalama Saturation = 15.5%, RGB kanalları arası fark (max-min) = 14.8
- Ölü balıklar her zaman daha "gri" ve "soluk" görünür
- İkon boyutu yaklaşık 32x32 piksel
- Arka plan siyah veya şeffaf olabilir (V < 20 olan pikselleri filtrele)

Lütfen şu fonksiyonu yaz:
bool IsFishDead(Mat fishIconRoi)
- Sadece HSV Saturation ve RGB kanalları arası fark (saturation diff) kullanarak
- Arka plan piksellerini filtreleyerek
- Basit ve hızlı olsun (envanterde çok sayıda slot taranacak)
```

---

### Prompt 5: Mevcut Template Matching Sistemine Renk Doğrulama Ekleme

```
Mevcut bir C# uygulamasında OpenCvSharp4 ile template matching yapıyorum.
Aşağıdaki mevcut metodlarım var:

```csharp
// Mevcut: Tek şablonla eşleştirme
public static TemplateMatchResult Match(
    Mat sourceMat, string templateRelativePath,
    double threshold = 0.85, bool useGrayscale = true,
    TemplateMatchModes mode = TemplateMatchModes.CCoeffNormed)

// Mevcut: Çoklu şablon, çoklu kopya bulma
public static List<TemplateMatchResult> FindAllMatches(
    Mat sourceMat, IEnumerable<string> candidateTemplatePaths,
    double threshold = 0.85, int maxMatchesPerTemplate = 100,
    bool useGrayscale = true)
```

Sorun: Aynı balığın Normal, Ölü ve Izgara halleri şekil olarak aynı.
Template matching yanlış durumu yüksek skor ile eşleştirebiliyor.

İstediğim: Mevcut `FindAllMatches` sonuçlarını alıp, her eşleşme için ek bir
renk doğrulama adımı uygulayan bir wrapper metod yaz:

```csharp
public static List<(TemplateMatchResult Match, FishState State)> FindAllMatchesWithColorValidation(
    Mat sourceMat, IEnumerable<string> candidateTemplatePaths,
    double threshold = 0.85)
```

Bu metod:
1. `FindAllMatches(sourceMat, candidateTemplatePaths, threshold, useGrayscale: false)` çağırsın
2. Her sonuç için sourceMat'ten ROI kırpsın (match.Bounds kullanarak)
3. ROI'nin HSV Saturation ortalamasını, R/G oranını, R/B oranını hesaplasın
4. Bu metriklere göre FishState (Normal/Dead/Grilled) belirlesin
5. Template adı ile renk analizi uyuşmuyorsa → renk analizine güvensin ve loglasın

Renk eşikleri:
- Ölü: HSV Saturation < 20% (OpenCV: S < 51/255), SatDiff (maxRGB-minRGB) < 20
- Izgara: Hue < 55° (OpenCV H < 28), R/B oranı > 1.6, Value < 45% (OpenCV V < 115)
- Normal: Yukarıdakilerin hiçbirine uymayan
```

---

## 8. Özet Karar Tablosu

```
┌─────────────────────────────────────────────────────────────┐
│                  BALIK DURUMU BELİRLEME                     │
├───────────────┬──────────┬──────────┬──────────┬────────────┤
│  Kontrol      │ Normal   │  Ölü     │ Izgara   │  Birim     │
├───────────────┼──────────┼──────────┼──────────┼────────────┤
│ Saturation    │  > 20%   │  < 20%   │  > 30%   │  HSV-S %   │
│ SatDiff       │  > 20    │  < 20    │  > 30    │  maxRGB-min│
│ Hue           │  > 55°   │  any     │  < 55°   │  HSV-H °   │
│ R/B Oranı     │  < 1.6   │  ~1.15   │  > 1.6   │  ratio     │
│ Parlaklık     │  > 100   │  ~100    │  < 90    │  avg RGB   │
│ R/G Oranı     │  ~1.12   │  ~1.06   │  > 1.3   │  ratio     │
└───────────────┴──────────┴──────────┴──────────┴────────────┘

Karar Sırası:
  1. Saturation < 20% VE SatDiff < 20  →  ÖLÜ
  2. Hue < 55° VE R/B > 1.6 VE V < 45% →  IZGARA  
  3. Diğer tüm durumlar                →  NORMAL
```
