# 📘 Aether WinForms - Proje Yapısı ve State Yönetimi Tutorial Dokümanı

Bu doküman, **Aether** projesinin dosya hiyerarşisini ("Nerede ne var"), **ClientState** mimarisini, **Renk Sabitlerini** ve bu sistemi adım adım nasıl kullanacağınızı anlatan kapsamlı bir rehberdir.

---

## 🗂️ 1. Proje Dizin Haritası ("Nerede Ne Var?")

Projede oluşturulan klasörler, dosyalar ve sorumlulukları aşağıda listelenmiştir:

```
Aether/
│
├── 📜 Program.cs                 --> [Giriş Noktası] Uygulama başladığında State'i ilk çalıştıran dosya
├── 📘 STATE_SYSTEM_GUIDE.md      --> [Rehber] Okuduğunuz tutorial ve mimari dokümanı
│
├── 📂 Constants/                  --> [Sabitler] Proje genelinde kullanılan sabit değerler
│   └── 🎨 Colors.cs              --> Renk paleti sabitleri (RGB / Hex tanımları)
│
├── 📂 States/                     --> [State Klasörü] Merkezi durum yönetimi
│   └── ⚡ ClientState.cs          --> Singleton State yönetim sınıfı & Event yayıncısı
│
├── 📂 Controls/                   --> [Kullanıcı Kontrolleri] Yeniden kullanılabilir arayüz bileşenleri
│   ├── 🎴 ClientCard.cs          --> Tek bir Client satırı/kartı (İsim, Checkbox, Başlat butonu)
│   ├── 🎴 ClientCard.Designer.cs --> ClientCard'ın UI görsel tasarım bileşenleri
│   ├── 📜 CustomScrollBar.cs     --> Colors sınıfını kullanan özel modern dikey ScrollBar
│   ├── 📱 ClientsControl.cs      --> Sol menüdeki Client listesi kapsayıcısı (FlowLayoutPanel)
│   └── 📱 ClientsControl.Designer.cs --> ClientsControl'ün UI tasarımı
│
└── 📂 Forms/                      --> [Ekranlar / Formlar] Ana uygulama pencereleri
    ├── 🖥️ MainForm.cs             --> Ana form arka kodları (Code-behind)
    └── 🖥️ MainForm.Designer.cs    --> Sol paneli (ClientsControl) ve sağ ayar panelini barındıran düzen
```

---

## 🔍 2. Bileşen Özellikleri ve Detayları

### 1. `Program.cs` (Uygulama Başlangıcı)
* **Görevi:** Program henüz ilk başladığında (`MainForm` bile oluşturulmadan önce) `ClientState.Initialize()` çağırarak state mekanizmasını ayağa kaldırır.

### 2. `Constants/Colors.cs` (Renk Paleti)
* **Görevi:** Tasarım uyumu için ortak renkleri tek merkezde toplar.
* **Mevcut Renkler:**
  - `Colors.MaviKoyu` (`RGB: 0, 177, 255` / `#00B1FF`)
  - `Colors.MaviAcik` (`RGB: 89, 189, 255` / `#59BDFF`)
  - `Colors.PembeKoyu` (`RGB: 244, 103, 136` / `#F46788`)
  - `Colors.PembeAcik` (`RGB: 255, 139, 164` / `#FF8BA4`)
  - `Colors.YesilKoyu` (`RGB: 99, 168, 71` / `#63A847`)
  - `Colors.YesilAcik` (`RGB: 135, 193, 109` / `#87C16D`)

### 3. `States/ClientState.cs` (Merkezi Veri Deposu)
* **Görevi:** Tıklanan kartı ve işaretlenen checkbox'ların durumunu merkezi olarak saklar ve değiştiğinde tüm uygulamaya haber verir.
* **Önemli Özellikler (Properties):**
  - `ClientState.Instance.SelectedClient` : Tıklanarak seçilen `ClientCard?` nesnesi.
  - `ClientState.Instance.CheckedClients` : Checkbox'ı işaretli olan `List<ClientCard>` kartlar.
* **Önemli Olaylar (Events):**
  - `OnSelectedClientChanged` : Aktif tıklanan kart değiştiğinde tetiklenir.
  - `OnCheckedClientsChanged` : İşaretli kartlar listesi değiştiğinde tetiklenir.

### 4. `Controls/ClientCard.cs` (Kart Satırı)
* **Özellikleri:**
  - `ClientName` (string) : Kartın adı ("Client 1", "Client 2" vb.)
  - `ClientNumber` (int) : Client ID / Sıra no.
  - `IsSelected` (bool) : Kart seçilince kenarlığını `Colors.YesilKoyu` yapar.
  - `IsChecked` (bool) : Checkbox işaretli mi/değil mi.
* **Olaylar:**
  - `OnCardSelected` : Karta tıklandığında tetiklenir.
  - `OnCheckedChanged` : Checkbox kutusu değiştiğinde tetiklenir.

### 5. `Controls/ClientsControl.cs` (Sol Panel Liste Kontrolü)
* **Görevi:** 10 adet `ClientCard` nesnesini dikey scroll destekli `FlowLayoutPanel` içinde oluşturur, karta tıklama ve checkbox değişimlerini dinleyip `ClientState.Instance` üzerine yansıtır.

---

## 🚀 3. Adım Adım Tutorial (Nasıl Kullanılır?)

### 🎓 Senaryo 1: Herhangi Bir Formdan Seçili Olan Kartın Bilgisini Okuma

Programın herhangi bir yerinden (örneğin bir `Button_Click` içinde) tıklanarak seçilmiş kartı veya işaretlenmiş tüm kartları okumak çok kolaydır:

```csharp
using System;
using System.Windows.Forms;
using Aether.States;   // <-- State namespace'ini ekleyin
using Aether.Controls; // <-- ClientCard tipi için

namespace Aether.Forms
{
    public partial class RaporForm : Form
    {
        public RaporForm()
        {
            InitializeComponent();
        }

        private void btnIslemYap_Click(object sender, EventArgs e)
        {
            // 1. Tıklanarak seçilen kartı alalım:
            ClientCard? secilenKart = ClientState.Instance.SelectedClient;

            if (secilenKart != null)
            {
                MessageBox.Show($"Aktif Seçili Kart: {secilenKart.ClientName}");
            }
            else
            {
                MessageBox.Show("Henüz hiçbir kart seçilmedi!");
            }

            // 2. Checkbox'ı işaretlenmiş kartların listesini alalım:
            List<ClientCard> isaretliKartlar = ClientState.Instance.CheckedClients;

            MessageBox.Show($"Toplam İşaretli Kart Sayısı: {isaretliKartlar.Count}");
        }
    }
}
```

---

### 🎓 Senaryo 2: State Değiştiğinde Formu Otomatik Güncelleme (Event Dinleme)

Bir form açıkken kullanıcı sol menüden farklı bir kart seçtiğinde veya checkbox işaretlediğinde formunuzun anında tepki vermesini istiyorsanız Event'lere abone olabilirsiniz:

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Aether.States;
using Aether.Controls;

namespace Aether.Forms
{
    public partial class DetayForm : Form
    {
        public DetayForm()
        {
            InitializeComponent();
        }

        private void DetayForm_Load(object sender, EventArgs e)
        {
            // 1. Tıklanan kart değiştiğinde çalışacak metodu bağla:
            ClientState.Instance.OnSelectedClientChanged += ClientState_OnSelectedClientChanged;

            // 2. Checkbox işaretleri değiştiğinde çalışacak metodu bağla:
            ClientState.Instance.OnCheckedClientsChanged += ClientState_OnCheckedClientsChanged;
        }

        // Tıklanan kart değişince anında buraya düşer
        private void ClientState_OnSelectedClientChanged(object? sender, ClientCard? card)
        {
            if (card != null)
            {
                labelClientAdi.Text = $"Seçilen: {card.ClientName}";
            }
        }

        // Checkbox durumları değişince anında buraya düşer
        private void ClientState_OnCheckedClientsChanged(object? sender, IReadOnlyList<ClientCard> checkedList)
        {
            labelIsaretliSayisi.Text = $"Seçili Adet: {checkedList.Count}";
        }

        // Form kapatılırken dinleyicileri temizlemek iyi bir pratiktir
        private void DetayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClientState.Instance.OnSelectedClientChanged -= ClientState_OnSelectedClientChanged;
            ClientState.Instance.OnCheckedClientsChanged -= ClientState_OnCheckedClientsChanged;
        }
    }
}
```

---

### 🎓 Senaryo 3: Renk Sabitlerini Kullanma (`Colors.cs`)

Form veya kontrollerinizde renk kodlarını elle `Color.FromArgb(...)` yazmak yerine sabit renk sınıfını kullanabilirsiniz:

```csharp
using Aether.Constants; // <-- Colors için ekleyin

private void FormTasarimDuzenle()
{
    // Arka plan ve kenarlık renklerini sabitten alma:
    this.BackColor = Colors.MaviKoyu;
    panelHead.BackColor = Colors.PembeKoyu;
    lblBaslik.ForeColor = Colors.YesilAcik;
}
```

---

## 🛠️ 4. Yaşam Döngüsü Akış Şeması (Flow)

```
[Program.cs başladı]
       │
       ▼
[ClientState.Initialize()] ──> State ilk değerleriyle hazırlandı.
       │
       ▼
[MainForm açıldı] ──> [ClientsControl yüklendi] ──> 10 Adet ClientCard oluşturuldu.
       │
       ├─► [Kullanıcı bir Karta tıkladı]
       │         │
       │         ▼
       │   ClientCard.OnCardSelected tetiklendi.
       │         │
       │         ▼
       │   ClientState.Instance.SelectedClient güncellendi.
       │         │
       │         ▼
       │   OnSelectedClientChanged abonelerine duyuruldu! 📣
       │
       └─► [Kullanıcı bir Checkbox işaretledi]
                 │
                 ▼
           ClientCard.OnCheckedChanged tetiklendi.
                 │
                 ▼
           ClientState.Instance.CheckedClients güncellendi.
                 │
                 ▼
           OnCheckedClientsChanged abonelerine duyuruldu! 📣
```

---

## ✅ Özet

1. **State ilk nerede başlar?** `Program.cs` içinde `ClientState.Initialize()` ile.
2. **Kart tıklamaları nerede saklanır?** `ClientState.Instance.SelectedClient`
3. **Checkbox verileri nerede saklanır?** `ClientState.Instance.CheckedClients`
4. **Renk sabitleri nerede?** `Aether.Constants.Colors`
5. **Sol panel kontrolü nerede?** `Aether.Controls.ClientsControl`
