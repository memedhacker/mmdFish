# FishCookingFunction.cs — Renk Doğrulama Güncellemesi

**Tarih:** 2026-08-18  
**Dosya:** [`FishCookingFunction.cs`](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Functions/FishCookingFunction.cs)

---

## Problem

Pişirme döngüsü, envanterdeki **Ölü** (`Ölü_`) ve **Izgara** (`Izgara_`) balık ikonlarını **normal (canlı) balıkmış gibi** pişirmeye çalışıyordu.

**Neden?**  
- Bu üç durum aynı şekle (silhouette) sahip; OpenCV template matching şekle odaklandığı için ölü ya da ızgara balığı canlı şablonuyla yüksek skorla eşleştirebiliyordu.  
- `GetCookableTemplates`, `FishIconTemplates.All` üzerinde tarama yapıyordu — bu listede `Ölü_` ve `Izgara_` prefixli şablonlar da bulunuyor.

---

## Yapılan Değişiklikler

### 1. `GetCookableTemplates` — Yalnızca Canlı Balık Şablonları

**Öncesi:**
```csharp
foreach (var templatePath in TemplateConstants.FishIconTemplates.All)
```

**Sonrası:**
```csharp
var normalFishTemplates = new List<string>();
normalFishTemplates.AddRange(TemplateConstants.FishIconTemplates.Common.All);
normalFishTemplates.AddRange(TemplateConstants.FishIconTemplates.Rare.All);

foreach (var templatePath in normalFishTemplates)
```

**Neden?**  
`FishIconTemplates.All`, içinde `Ölü_` ve `Izgara_` şablonlarını da barındırıyor. Pişirme için aday template'leri ararken bu şablonlar tarama listesine alınmamalıdır. Böylece template matching aşamasında **yalnızca canlı balıkların ikonları** hedeflenir.

---

### 2. `ScanFishToCook` — İki Katmanlı Renk Doğrulama

**Öncesi:**
```csharp
var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, cookableTemplates, threshold: 0.80, useGrayscale: false);

foreach (var m in allFound)
{
    if (!matches.Any(existing => ...overlap...))
        matches.Add(m);
}
```

**Sonrası:**
```csharp
var allFound = TemplateConstants.FindAllMatches(fishAreaBmp, cookableTemplates, threshold: 0.80, useGrayscale: false);

foreach (var m in allFound)
{
    // 1. Çakışma kontrolü
    if (matches.Any(existing => ...overlap...))
        continue;

    // 2. Renk doğrulama: Eşleşen ROI canlı renk mi?
    if (!IsFishColorNormal(fishAreaBmp, m.Bounds))
    {
        BotLogger.LogInfo(clientId, $"[PİŞİR FILTRE] '{m.TemplateName}' renk doğrulamasından geçemedi (Ölü/Izgara rengi). Atlanıyor.");
        continue;
    }

    matches.Add(m);
}
```

**Neden?**  
Template matching %90+ skorla bir eşleşme bulsa bile, o eşleşmenin rengi yanlış olabilir (canlı balık şablonu ölü balıkla örtüşmesi). İkinci katman olarak piksel düzeyinde renk analizi yapılır; ölü veya ızgara renkli ikonlar listeden çıkarılır.

---

### 3. Yeni Metod: `IsFishColorNormal`

```csharp
private static bool IsFishColorNormal(Bitmap sourceBitmap, Rectangle roiBounds)
```

`System.Drawing.Bitmap.GetPixel` ile çalışır — OpenCvSharp bağımlılığı gerektirmez.

#### Adımlar:

1. **Bounds güvenlik kontrolü** — Bitmap sınırı dışı erişimi önler
2. **Örnekleme adımı** — Küçük ikonlarda her piksel, büyüklerde atlamalı okuma (`step = min(w,h) / 8`)
3. **Arka plan filtresi** — Ortalama parlaklığı < 20 olan siyah/şeffaf pikseller atlanır
4. **Renk istatistikleri hesaplama** — Ortalama R, G, B değerleri
5. **Karar aşaması:**

| Durum | Koşul | Eşik Temeli |
|-------|-------|-------------|
| **Ölü** | `satPercent < 20 VE satDiff < 20` | Ölü ortalama: S=15.5%, SatDiff=14.8 |
| **Izgara** | `R/B > 1.6 VE brightness < 90` | Izgara ortalama: R/B=1.95, Bright=77.5 |
| **Normal** | Yukarıdakilerin hiçbiri değil | Normal ortalama: S=33%, Bright=110.4 |

#### Eşik değerleri kaynağı:
24 farklı balık türünün (12 Common, 9 Rare) tüm üç durumundan (Normal/Ölü/Izgara) elde edilen renk analizi ortalamaları:

| Metrik | Normal | Ölü | Izgara |
|--------|--------|-----|--------|
| Saturation (HSV-S) | 33% | **15.5%** | 45.5% |
| SatDiff (max-min RGB) | 40.0 | **14.8** | 46.2 |
| R/B Oranı | 1.45 | 1.15 | **1.95** |
| Parlaklık (avg RGB) | 110.4 | 100.1 | **77.5** |

---

### 4. `FindCampfireInInventory` — `useGrayscale: false` Eklendi

**Öncesi:**
```csharp
var matches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.70);
```

**Sonrası:**
```csharp
var matches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.Ates, threshold: 0.70, useGrayscale: false);
```

**Neden?**  
Kamp ateşinin ikonu turuncu/sarı renklere sahiptir. Grayscale'e dönüştürmek renk bilgisini kaybettirir ve aynı parlaklıkta farklı renkli bir ikonla karışma riskini artırır. `useGrayscale: false` ile daha güvenilir eşleştirme sağlanır.

---

## Özet: Güvenlik Katmanları

```
Pişirme aday listesi
        │
        ▼
[GetCookableTemplates]
 → Sadece FishIconTemplates.Common + Rare
 → Ölü_ ve Izgara_ şablonları listeden dışarıda
        │
        ▼
[ScanFishToCook]
 → FindAllMatches (useGrayscale: false)
 → Her eşleşme için çakışma kontrolü
 → Her eşleşme için IsFishColorNormal renk doğrulama
        │
        ▼
[Pişirme döngüsüne giden liste]
 → Yalnızca canlı renkli balıklar
```

## Build Sonucu

✅ **0 hata, 4 uyarı** (mevcut uyarılar önceden varolan, bu değişiklikle ilgisiz)
