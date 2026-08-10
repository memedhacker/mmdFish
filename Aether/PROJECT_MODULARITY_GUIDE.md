# Aether Proje Mimari Rehberi

> **Son Güncelleme:** Modülerlik refactoring'i sonrası (Ağustos 2026)
> Bu dosya projenin tüm katmanlı yapısını, tasarım kararlarını ve ekleme/değiştirme kurallarını belgeler.

---

## 📁 Klasör Yapısı

```
Aether/
├── Assets/
│   └── FishIcons/
│       ├── rare/        ← Nadir balık PNG'leri
│       └── common/      ← Yaygın balık PNG'leri
│
├── Constants/
│   └── Colors.cs        ← Proje renk paleti (static)
│
├── Controls/
│   ├── ClientCard.cs    ← Tekil client kartı (UserControl)
│   ├── ClientsControl.cs ← Client kartları listesi (UserControl)
│   ├── CustomScrollBar.cs ← Özel kaydırma çubuğu (UserControl)
│   └── DoubleBufferedFlowLayoutPanel.cs ← Titreşimsiz panel
│
├── Forms/
│   └── MainForm.cs      ← Uygulama ana penceresi (shell/navigator)
│
├── Helpers/
│   └── FishFilterTableBuilder.cs ← FishBot filtresi için dinamik tablo oluşturucu (static)
│
├── Models/
│   ├── ClientModel.cs   ← Client veri modeli (domain)
│   └── ClientInfo.cs    ← State katmanı için hafif DTO
│
├── Native/
│   └── Win32Native.cs   ← Win32 API P/Invoke merkezi
│
├── Pages/
│   ├── BaseBotPage.cs   ← Tüm bot sayfaları için soyut taban
│   ├── FishBotPage.cs   ← Balık botu sayfası
│   ├── AlchemyPage.cs   ← Simya botu sayfası
│   ├── AntiBanPage.cs   ← Ban koruması sayfası
│   ├── UpgradePage.cs   ← Oto artı basma sayfası
│   └── FishPuzzlePage.cs ← Puzzle botu sayfası
│
├── Services/
│   ├── ClientService.cs       ← Client listesi üretimi (static)
│   └── InputAutomationService.cs ← Klavye/fare simülasyonu (singleton)
│
└── States/
    ├── ClientState.cs   ← Seçim ve bot durumları (singleton)
    └── PageState.cs     ← Aktif sayfa durumu (singleton)
```

---

## 🏛️ Katman Mimarisi

```
┌────────────────────────────────────────────┐
│  Forms (MainForm)                          │  ← Shell: navigasyon, selectAll
├────────────────────────────────────────────┤
│  Pages (BaseBotPage → FishBotPage vd.)     │  ← Sayfa içeriği, UI mantığı
├────────────────────────────────────────────┤
│  Controls (ClientsControl, ClientCard vd.) │  ← Yeniden kullanılabilir UI
├────────────────────────────────────────────┤
│  States (ClientState, PageState)           │  ← Merkezi durum yönetimi
├────────────────────────────────────────────┤
│  Services (ClientService, InputAutomation) │  ← İş mantığı, otomasyon
├────────────────────────────────────────────┤
│  Models (ClientModel, ClientInfo)          │  ← Saf veri nesneleri
├────────────────────────────────────────────┤
│  Native (Win32Native)                      │  ← Platform API erişimi
└────────────────────────────────────────────┘
```

**Bağımlılık yönü:** Yukarı katmanlar aşağıdakilere bağımlıdır. Aşağı katmanlar (States, Models, Native) UI katmanlarına (Controls, Pages, Forms) **asla** bağımlı değildir.

---

## 🔑 Temel Tasarım Kararları

### 1. ClientInfo — State Katmanının DTO'su

`ClientState` artık UI kontrolü olan `ClientCard` yerine `ClientInfo` kullanır. Bu, `States → Controls` katman bağımlılığını ortadan kaldırır.

```csharp
// ClientState içinde: ClientCard DEĞİL, ClientInfo kullanılır
public ClientInfo? SelectedClient { get; set; }
public List<ClientInfo> CheckedClients { get; }
```

`ClientInfo` yalnızca `int Id` ve `string Name` barındırır. Görsel seçim durumu (`IsSelected`) `ClientsControl` içinde `_currentlySelectedCard` field'ı ile yönetilir.

### 2. BaseBotPage — Sayfa Taban Sınıfı

Tüm bot sayfaları `BaseBotPage`'den türetilir. Bu sınıf:
- `ClientState.OnSelectedClientChanged` aboneliğini yönetir
- Client adını thread-safe şekilde günceller
- Handle yok edildiğinde event'i temizler

```csharp
// Yeni sayfa ekleme şablonu:
public partial class YeniSayfa : BaseBotPage
{
    public YeniSayfa() => InitializeComponent();
    protected override Label ClientNameLabel => clientNameLabel;
    // Ekstra OnLoad davranışı varsa:
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e); // ZORUNLU: client binding burada başlar
        // ... ek başlatma kodu
    }
}
```

### 3. ClientState — Singleton Gözlemci

`ClientState.Instance` üç ana event yayar:

| Event | Tip | Ne zaman |
|-------|-----|----------|
| `OnSelectedClientChanged` | `EventHandler<ClientInfo?>` | Kart tıklandığında |
| `OnCheckedClientsChanged` | `EventHandler<IReadOnlyList<ClientInfo>>` | Checkbox değiştiğinde |
| `OnBotStateChanged` | `EventHandler` | Bot başlat/durdurulduğunda |

### 4. ClientService — Static Utility

`ClientService`, state gerektirmeyen saf üretim fonksiyonu olduğu için `static class` olarak tasarlanmıştır:

```csharp
var clients = ClientService.GenerateDefaultClients(10); // instance gerekmez
```

### 5. MainForm — Navigasyon Shell'i

`MainForm`, sayfa geçişlerini `_pageMap` dictionary'si aracılığıyla yönetir. Yeni bir sayfa eklemek için:

```csharp
// MainForm.cs → RegisterPageButtons() içine ekle:
_pageMap["YeniSayfa"] = (pYeniSayfaButton, () => new YeniSayfa());
```

`NavigateToPage` metodu:
1. `PageState.CurrentPage`'i günceller
2. İlgili butonu disable eder
3. Önceki sayfayı temizler ve yenisini yükler
4. `pageScrollBar`'ı senkronize eder

**NOT:** `PageState.OnPageChanged` event'ine `MainForm` **abone değildir**. Navigasyon doğrudan `NavigateToPage` çağrısıyla tetiklenir. `PageState.CurrentPage`, harici bileşenlerin aktif sayfayı okuması için tutulur.

---

## 🎨 Renk Kullanımı

Tüm renkler `Constants/Colors.cs` üzerinden kullanılmalıdır:

```csharp
using Aether.Constants;

control.BackColor = Colors.MaviKoyu;   // #00B1FF
control.ForeColor = Colors.PembeAcik;  // #FF8BA4
panel.RectColor   = Colors.YesilAcik;  // #87C16D
```

| Sabit | Renk | Hex |
|-------|------|-----|
| `MaviKoyu` | Mavi Koyu | `#00B1FF` |
| `MaviAcik` | Mavi Açık | `#59BDFF` |
| `PembeKoyu` | Pembe Koyu | `#F46788` |
| `PembeAcik` | Pembe Açık | `#FF8BA4` |
| `YesilKoyu` | Yeşil Koyu | `#63A847` |
| `YesilAcik` | Yeşil Açık | `#87C16D` |

---

## ➕ Yeni Özellik Ekleme Rehberi

### Yeni Bot Sayfası Eklemek

1. `Pages/` altında `YeniPage.cs` ve `YeniPage.Designer.cs` oluştur
2. `YeniPage : BaseBotPage` şeklinde türet
3. `protected override Label ClientNameLabel => clientNameLabel;` ekle
4. `MainForm.RegisterPageButtons()` içine buton + sayfa eşlemesini ekle
5. `MainForm.Designer.cs` içine yeni butonu ekle

### Yeni Bot State Eklemek

1. `ClientState.cs` içine `private bool _isYeniBot = false;` field ekle
2. Property ekle: `get/set` içinde `OnBotStateChanged?.Invoke(...)` çağır
3. `Reset()` metoduna `_isYeniBot = false;` satırı ekle

### Yeni Servis Eklemek

- Stateless ise → `static class` olarak yaz
- State gerektiriyorsa → `Lazy<T>` singleton pattern kullan (`ClientState` örnek)
- Win32 API kullanıyorsa → `Native/Win32Native.cs`'e ekle, servisten çağır

---

## 📝 Yapılan Değişiklikler (Modülerlik Refactoring)

### Yeni Dosyalar
| Dosya | Açıklama |
|-------|----------|
| `Models/ClientInfo.cs` | State katmanı için UI bağımsız client DTO'su |
| `Pages/BaseBotPage.cs` | Tüm bot sayfaları için ortak taban sınıf |

### Güncellenen Dosyalar
| Dosya | Değişiklik |
|-------|------------|
| `States/ClientState.cs` | `ClientCard` → `ClientInfo`; katman bağımlılığı giderildi |
| `Controls/ClientsControl.cs` | Kullanılmayan 2 event kaldırıldı; `_currentlySelectedCard` ile görsel takip; `ClientInfo` entegrasyonu |
| `Controls/ClientCard.cs` | 4 bot state property kaldırıldı (ClientModel ve ClientState ile çakışıyordu) |
| `Controls/CustomScrollBar.cs` | `[DesignerSerializationVisibility(Hidden)]` attribute eklendi (WFO1000 hataları giderildi) |
| `Pages/AlchemyPage.cs` | 51 satır → 12 satır; `BaseBotPage`'den türetildi |
| `Pages/AntiBanPage.cs` | 51 satır → 12 satır; `BaseBotPage`'den türetildi |
| `Pages/UpgradePage.cs` | 51 satır → 12 satır; `BaseBotPage`'den türetildi |
| `Pages/FishPuzzlePage.cs` | 51 satır → 12 satır; `BaseBotPage`'den türetildi |
| `Pages/FishBotPage.cs` | `BaseBotPage`'den türetildi; `BuildFishFilterTable` → `OnLoad`'a alındı; `Colors.cs` entegrasyonu |
| `Forms/MainForm.cs` | Kullanılmayan `SelectedClient`/`CheckedClients` proxy'leri kaldırıldı; `PageState` döngüsel aboneliği kaldırıldı; event tipi `ClientInfo`'ya güncellendi |
| `Services/ClientService.cs` | Gereksiz singleton → `static class`'a dönüştürüldü |
