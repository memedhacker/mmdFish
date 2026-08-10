# 🏛️ Aether WinForms - Modüler Mimari & Katmanlı Yapı Rehberi

Bu doküman, **Aether** projesinin baştan sona yeniden yapılandırılarak modüler hale getirilmiş yeni katmanlı mimarisini, dosya sorumluluklarını ve geliştirici kullanım rehberini içermektedir.

---

## 🗂️ 1. Yenilenmiş Proje Katman Haritası

Proje, **SoC (Separation of Concerns - Sorumlulukların Ayrılması)** ilkesine uygun olarak 6 ana katmana bölünmüştür:

```
Aether/
│
├── 📜 Program.cs                     --> Uygulama giriş noktası & State erken başlatma
├── 📘 PROJECT_MODULARITY_GUIDE.md    --> Proje mimari ve geliştirici rehberi (Bu dosya)
├── 📘 STATE_SYSTEM_GUIDE.md          --> State sistemi detay kullanım kılavuzu
│
├── 📂 Native/                         --> [Yerel İşletim Sistemi Katmanı]
│   └── ⚙️ Win32Native.cs              --> Tüm Win32 user32.dll P/Invoke API ve Sabitleri
│
├── 📂 Models/                         --> [Veri Modelleri Katmanı]
│   └── 📦 ClientModel.cs              --> Client domain veri sınıfı (Id, Name, Status vb.)
│
├── 📂 Services/                       --> [İş Mantığı ve Servisler Katmanı]
│   ├── 🛠️ ClientService.cs            --> Client veri üretimi ve iş mantığı servisi
│   └── 🤖 InputAutomationService.cs    --> Donanımsal klavye/fare makro otomasyon servisi
│
├── 📂 States/                         --> [Reaktif Durum Yönetimi Katmanı]
│   ├── ⚡ ClientState.cs              --> Singleton + Observer pattern istemci durum deposu
│   └── 📄 PageState.cs                --> Aktif sayfa yönlendirme durum deposu (CurrentPage)
│
├── 📂 Pages/                          --> [Sayfa Kontrolleri Katmanı]
│   ├── 🎣 FishBotPage.cs              --> Balık Botu ayarlar sayfası
│   ├── 🧩 FishPuzzlePage.cs           --> Puzzle Botu ayarlar sayfası
│   ├── 🧪 AlchemyPage.cs              --> Simya Botu ayarlar sayfası
│   ├── ⚔️ UpgradePage.cs              --> Oto Artı Basma ayarlar sayfası
│   └── 🛡️ AntiBanPage.cs              --> Ban Koruması ayarlar sayfası
│
├── 📂 Constants/                      --> [Sabitler Katmanı]
│   └── 🎨 Colors.cs                  --> Hex & RGB renk paleti sabitleri
│
├── 📂 Controls/                       --> [Kullanıcı Arayüzü Bileşenleri]
│   ├── 🎴 ClientCard.cs              --> Tekil Client kart satırı bileşeni
│   ├── 📜 CustomScrollBar.cs         --> Colors sınıfı ile çizilen özel pürüzsüz ScrollBar
│   ├── 🌊 DoubleBufferedFlowLayoutPanel.cs --> Titremesiz (DoubleBuffered) özel FlowPanel
│   └── 📱 ClientsControl.cs          --> Sol menüdeki Client listesi kapsayıcı kontrolü
│
└── 📂 Forms/                          --> [Arayüz Ekranları Katmanı]
    ├── 🖥️ MainForm.cs                 --> Ana uygulama arayüz formu
    ├── 🎯 Form_Overlay.cs             --> Donanımsal imleç takip görselleştirme katmanı
    └── ⚙️ Form1.cs                    --> Tarama ve otomasyon formu
```

---

## 🔍 2. Katmanlar ve Sorumlulukları

### 1. `Native` Katmanı (`Win32Native.cs`)
- Projede dağınık halde bulunan tüm `[DllImport("user32.dll")]` fonksiyonları ve hexadecimal sabitler (`WM_HOTKEY`, `SB_VERT`, `GWL_EXSTYLE`, `VK_SPACE` vb.) tek bir sınıfta toplandı.
- Kod tekrarı önlendi ve işletim sistemi çağrıları güvenli bir yapıya kavuşturuldu.

### 2. `Models` Katmanı (`ClientModel.cs`)
- Client verilerini temsil eden saf veri sınıfı. (`Id`, `Name`, `IsSelected`, `IsChecked`, `IsFishBotRunning`, `IsUpgradeBotRunning`, `IsFishPuzzleRunning`, `IsAlchemyRunning`).

### 3. `Services` Katmanı (`ClientService.cs`, `InputAutomationService.cs`)
- **`ClientService`**: Veri üretimi ve istemci iş mantığını yönetir.
- **`InputAutomationService`**: Donanımsal klavye vuruşları (`keybd_event`) ve fare tıklamalarını (`mouse_event`) UI kodlarından tamamen ayırarak servis haline getirir.

### 4. `States` Katmanı (`ClientState.cs`)
- **Singleton + Observer** mimarisi ile `SelectedClient`, `CheckedClients` ve modül çalışma durumlarını (`IsFishBotRunning`, `IsUpgradeBotRunning`, `IsFishPuzzleRunning`, `IsAlchemyRunning`) varsayılan olarak `false` tutar.
- `Program.cs` aşamasında başlatılır. Değişimlerde `OnBotStateChanged` event duyuruları yapar.

### 5. `Constants` Katmanı (`Colors.cs`)
- Projenin renk paletini (`MaviKoyu`, `MaviAcik`, `PembeKoyu`, `PembeAcik`, `YesilKoyu`, `YesilAcik`) barındırır.

### 6. `Controls` Katmanı
- **`DoubleBufferedFlowLayoutPanel`**: Kaydırma esnasındaki titreme (flicker) ve görüntü bozulmalarını `DoubleBuffered = true` ve `WndProc` seviyesinde engeller.
- **`CustomScrollBar`**: `Colors` sınıfını kullanarak çizilen, yumuşak kaydırma sağlayan dikey scrollbar.
- **`ClientCard`**: `IsSelected` ve `IsChecked` durumlarını yansıtan kart satırı.
- **`ClientsControl`**: Kartları listeleyen ve `ClientState` ile senkron çalışan sol menü.

---

## 🔄 3. Mimari Akış ve İletişim Şeması

```
                    ┌─────────────────────────┐
                    │       Program.cs        │ (Uygulama Başlangıcı)
                    └────────────┬────────────┘
                                 │ ClientState.Initialize()
                                 ▼
                    ┌─────────────────────────┐
                    │      ClientState        │ (Merkezi State Deposu)
                    └────────────▲────────────┘
                                 │
           ┌─────────────────────┴─────────────────────┐
           │                                           │
┌──────────┴──────────┐                     ┌──────────┴──────────┐
│   ClientsControl    │                     │      MainForm       │
│    (UI Kontrolü)    │                     │      (Form)         │
└──────────┬──────────┘                     └─────────────────────┘
           │
           ├─► ClientService.Instance.GenerateDefaultClients()
           │
           ├─► ClientCard (x10)
           │
           └─► CustomScrollBar + DoubleBufferedFlowLayoutPanel
```

---

## 💻 4. Geliştirici Rehberi (Tutorial)

### 🔹 Servis Katmanını Kullanarak Makro Çalıştırma

Form veya arayüz kodları içerisinde Win32 API çağırmak yerine `InputAutomationService` kullanabilirsiniz:

```csharp
using Aether.Services;

// Donanımsal Makro Dizisini Asenkron Tetikleme:
await InputAutomationService.Instance.TriggerMacroSequenceAsync(logMsg =>
{
    Console.WriteLine(logMsg);
});

// Fare Sol Tıklama:
InputAutomationService.Instance.SendLeftClick();
```

---

### 🔹 State Verilerine Erişme ve Dinleme

```csharp
using Aether.States;

// O anki seçili olan kart:
var seciliKart = ClientState.Instance.SelectedClient;

// İşaretlenmiş kartlar listesi:
var isaretliler = ClientState.Instance.CheckedClients;

// State değişimine abone olma:
ClientState.Instance.OnSelectedClientChanged += (sender, card) =>
{
    // Kart değiştiğinde otomatik çalışır
};
```

---

## ✨ 5. Modülerlik Kazanımları

1. **Clean Code (Temiz Kod)**: Formlar artık sadece arayüz çizimi ile ilgilenir; Win32 API, iş mantığı ve state yönetimi kendi katmanlarına ayrılmıştır.
2. **Titremesiz ve Yüksek Performanslı Arayüz**: `DoubleBufferedFlowLayoutPanel` ve `CustomScrollBar` ile kaydırma performansı pürüzsüzleştirildi.
3. **Kolay Genişletilebilirlik**: Yeni bir modül veya servis eklemek için mevcut koda dokunmadan ilgili katmana yeni bir sınıf eklemek yeterlidir.
