# 🖼️ Şablon (Template) Ekleme ve Yönetim Rehberi

Bu kılavuz, **Aether** balık botu projesine yeni şablon görselleri (OpenCV Template Matching) ekleme, bunları [`TemplateConstants.cs`](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Constants/TemplateConstants.cs) içerisine kaydetme ve kod içerisinde kullanma adımlarını açıklar.

---

## 📁 1. Klasör Yapısı (`Assets/templates/`)

Tüm şablon PNG görselleri projenin `Assets/templates/` dizini altında kategorilerine göre saklanır:

```text
Assets/
└── templates/
    ├── autopass/          # Otomatik geçiş / doğrulama şablonları (minik_balik.png, vb.)
    ├── fishnames/         # Balık ve tutulan eşya şablonları (sazan.png, sudak.png, vb.)
    ├── waypoints/         # Sistem & durum bildirimleri (bisey_takildi.png, vb.)
    └── window_parts/      # Pencere parçaları, menü başlıkları (EquipmentMenuTitle.png, vb.)
```

> [!TIP]
> Yeni bir kategori oluşturmak isterseniz `Assets/templates/` altına yeni bir klasör açabilirsiniz (Örn: `Assets/templates/inventory/`).

---

## 🚀 2. Yeni Bir Şablon Ekleme Adımları (Adım Adım)

### Adım 1: Görseli Hazırlama ve Klasöre Kaydetme
1. Oyun içerisinden aramak istediğiniz nesnenin ekran görüntüsünü 1:1 piksel ölçeğinde kırpın.
2. PNG formatında kaydedin (Örn: `Assets/templates/waypoints/yeni_bildirim.png`).
3. Dosya isimlendirmesinde Türkçe karakter ve boşluk yerine alt çizgi (`_`) kullanın (Örn: `altin_yuzuk.png`).

---

### Adım 2: `TemplateConstants.cs` İçerisine Tanımlama

[`Constants/TemplateConstants.cs`](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Constants/TemplateConstants.cs) dosyasını açın:

#### A) Mevcut Bir Kategoriye Ekleme:
Örneğin `Waypoints` kategorisine yeni bir görsel ekliyorsanız:

```csharp
public static class Waypoints
{
    public const string BiseyTakildi = "waypoints/bisey_takildi.png";
    public const string YakalananBalik = "waypoints/yakalanan_balik.png";
    // ➕ YENİ EKLENEN:
    public const string YeniBildirim = "waypoints/yeni_bildirim.png";

    public static readonly IReadOnlyList<string> All = new[]
    {
        BiseyTakildi,
        YakalananBalik,
        YeniBildirim // ➕ Listeye ekleyin
    };
}
```

#### B) Tamamen Yeni Bir Kategori Oluşturma:
Yeni bir kategori (örneğin `Inventory`) ekliyorsanız:

```csharp
public static class Inventory
{
    public const string BosSlot = "inventory/bos_slot.png";
    public const string OltaSlot = "inventory/olta_slot.png";

    public static readonly IReadOnlyList<string> All = new[]
    {
        BosSlot,
        OltaSlot
    };
}
```

Ardından dosyanın altındaki static kurucu metoda (`static TemplateConstants()`) yeni kategorinizi dahil edin:

```csharp
static TemplateConstants()
{
    var all = new List<string>();
    all.AddRange(AutoPass.All);
    all.AddRange(FishNames.All);
    all.AddRange(Waypoints.All);
    all.AddRange(WindowParts.All);
    all.AddRange(Inventory.All); // ➕ Yeni kategori eklendi

    AllTemplates = all.AsReadOnly();
}
```

---

## 💻 3. Kod İçerisinde Kullanım Örnekleri

### Örnek 1: Tekil Şablon Arama (`Match`)
Belirli bir ekran görüntüsü üzerinde tek bir şablonun olup olmadığını kontrol etmek için:

```csharp
using Aether.Constants;
using Aether.Helpers;
using System.Drawing;

// 1. Ekran bölgesini yakala (Örn: ChatBox alanı)
using Bitmap? chatBmp = WindowRegionCaptureHelper.CaptureRegion(
    client.Handle, 
    RegionConstants.ChatBoxPosition
);

if (chatBmp != null)
{
    // 2. Şablonu ara (0.85 = %85 benzerlik eşiği)
    var result = TemplateConstants.Match(
        chatBmp, 
        TemplateConstants.Waypoints.BiseyTakildi, 
        threshold: 0.85
    );

    if (result.IsSuccess)
    {
        Debug.WriteLine($"✅ Şablon bulundu! Konum: {result.Location}, Benzerlik: %{result.Confidence * 100:F1}");
    }
}
```

---

### Örnek 2: Çoklu Şablon Listesi İçinden En İyisini Bulma (`FindBestMatch`)
Bir görsel üzerinde balık listesinden hangisinin tutulduğunu tespit etmek için:

```csharp
using Aether.Constants;

// Tüm balık isimleri arasında en yüksek güvenilirlikli olanı bulur
var bestFish = TemplateConstants.FindBestMatch(
    regionBmp, 
    TemplateConstants.FishNames.All, 
    minThreshold: 0.80
);

if (bestFish != null)
{
    Debug.WriteLine($"🐟 Yakalanan Balık: {bestFish.TemplateName} (Güven: %{bestFish.Confidence * 100:F1})");
}
```

---

## 🧪 4. Arayüzden Test Etme (Canlı Önizleme)

Eklediğiniz şablonları anında test etmek için:
1. Uygulamada istemcinizi seçin ve **"Ekranı Test Et"** penceresini açın.
2. Sağ alt kontrol panelindeki **"Kategori"** açılır listesinden ilgili şablon grubunu seçin (Örn: `Waypoints`, `FishNames`, `WindowParts` veya `Tümü`).
3. **"▶️ Canlı Testi Başlat"** butonuna basarak eşleşme oranlarını, tespit edilen koordinatları ve logları anlık olarak izleyin.
