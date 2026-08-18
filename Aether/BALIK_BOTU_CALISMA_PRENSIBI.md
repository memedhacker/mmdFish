# 🎣 Aether Balık Botu — Çalışma Prensibi ve Algoritma Kılavuzu

Bu belge, **Aether Balık Botu**'nun başlangıç hazırlığından asıl balık tutma döngüsüne, mini-oyun mekaniğinden hata yönetimine kadar tüm çalışma adımlarını ve algoritma mantığını detaylı olarak açıklar. 

Üzerinde istediğiniz adımları rahatça değiştirebilir, silebilir veya yeni mantıklar ekleyebilirsiniz.

---

## 📑 İÇİNDEKİLER

1. [Genel Mimari ve Başlangıç Hazırlık Sekansı (`FishBotStartupFunction.cs`)](#1-genel-mimari-ve-başlangıç-hazırlık-sekansı-fishbotstartupfunctioncs)
2. [Balıkçı NPC & Otomatik Yem Satın Alma (`StartupFishermanFunction.cs`)](#2-balıkçı-npc--otomatik-yem-satın-alma-startupfishermanfunctioncs)
3. [Asıl Balık Tutma Döngüsü (`FishingExecutionFunction.cs`)](#3-asıl-balık-tutma-döngüsü-fishingexecutionfunctioncs)
4. [Balık Yakalama Mini-Oyunu (`FishingMinigameFunction.cs`)](#4-balık-yakalama-mini-oyunu-fishingminigamefunctioncs)
5. [Animasyon İptali Mekanizması (`PerformAnimationCancelAsync`)](#5-animasyon-iptali-mekanizması-performanimationcancelasync)
6. [Önemli Yardımcı Fonksiyonlar ve Kısayollar](#6-önemli-yardımcı-fonksiyonlar-ve-kısayollar)

---

## 1. Genel Mimari ve Başlangıç Hazırlık Sekansı (`FishBotStartupFunction.cs`)

Bot başlatıldığında (**SADECE 1 KERE**) aşağıdaki 12 hazırlık adımı sırayla çalıştırılır:

```mermaid
graph TD
    A[Bot Başlatıldı] --> B[1. Oyun Penceresini En Öne Getir]
    B --> C[2. Ekran Ortasına Sağ Tıkla - Odaklan]
    C --> D[3. 'F' Tuşuna 3 sn Basılı Tut]
    D --> E[4. 'G' Tuşuna 3 sn Basılı Tut]
    E --> F[5. Ekipman Menüsü Açık mı? Kontrol Et / 'I' Tuşu]
    F --> G[6. Ekipman Menüsünü 'ExitButton' ile Kapat]
    G --> H[7. Ayarlanan Envanter Sayfasına Geç 'InventoryPage']
    H --> I[8. Envanterdeki Yemleri Stackle / Birleştir]
    I --> J[9. Kamp Ateşlerini İlk 3 Slota Düzenle]
    J --> K[10. Yemleri 4. Slot ve Sonrasına Taşı]
    K --> L[11. BuyWorm Aktifse Balıkçıdan Yem Al]
    L --> M[12. Envanter Boş Slot Kontrolü]
    M --> N{EmptySlot == 0?}
    N -- Evet --► O[🛑 Botu Durdur & MainForm Öne Getir]
    N -- Hayır --► P[🚀 Asıl Balık Tutma Döngüsüne Başla]
```

### 📋 Başlangıç Adım Detayları:
1. **Oyun Penceresine Odaklanma**: Hedef istemcinin HWND penceresi en öne getirilir ve ekran merkezine (`400, 300`) insansı kavisle sağ tıklanır.
2. **Kamera Açısı Ayarı**:
   - `F` tuşuna **3 saniye** kesintisiz basılı tutulup bırakılır.
   - `G` tuşuna **3 saniye** kesintisiz basılı tutulup bırakılır (Kamera kuşbakışı ideal açıya getirilir).
3. **Ekipman Menüsü Kontrolü**:
   - `EquipmentMenuTitlePosition` taranır. Menü açıksa `EquipmentMenuExitButton` tıklanarak kapatılır; kapalıysa `I` tuşuna basılıp kapatılması garanti edilir.
4. **Envanter Sayfası Seçimi**:
   - `InventoryPagesPosition` taranır. Kullanıcının seçtiği sayfa (örn: 1. Sayfa veya 2. Sayfa) aktif değilse o sayfaya tıklanır.
5. **Yemleri Stackleme (Birleştirme)**:
   - `InventoryBaitArea` içerisindeki tüm `yem.png` şablonları taranır (Adet: 200 olanlar hariç tutulur) ve birbirinin üstüne sürüklenip bırakılarak birleştirilir.
6. **Kamp Ateşleri Düzeni**:
   - `ates.png` şablonları taranarak envanterin ilk 3 slotuna düzenlenir.
7. **Yemleri Alt Slotlara Taşıma**:
   - Yemler 4. slot ve sonrasındaki boşluklara hizalanır.
8. **Balıkçı Yem Alımı**:
   - `BuyWormEnabled` aktifse ve 4.+ slotlarda boş yer varsa balıkçıdan yem satın alınır.
9. **Başlangıç Boş Slot Kontrolü**:
   - `InventoryFishArea` taranır. Eğer hiç boş yer yoksa (`EmptySlot == 0`) bot durdurulur ve MainForm öne getirilir.

---

## 2. Balıkçı NPC & Otomatik Yem Satın Alma (`StartupFishermanFunction.cs`)

Yem bittiğinde veya başlangıçta yem slotları boş olduğunda (`BuyWormEnabled == true`) otomatik olarak çalışır:

```mermaid
graph TD
    A[Yem Satın Alma Başlatıldı] --> B[Adım A: Boş Yem Slotlarını Say]
    B --> C{Boş Slot Var mı?}
    C -- Hayır --► Z[Market İşlemi Gerekmez / Çık]
    C -- Evet --► D[Adım B: Balıkçı NPC Ara FisherManSearchArea]
    D --> E{Balıkçı Bulundu mu?}
    E -- Hayır --► F[Kamerayı 20px Sağa Döndür Orta Tuşla]
    F --> D
    E -- Evet --► G[Adım F: Balıkçıya Tıkla 1sn Bekle]
    G --> H[Kontrol 1: 'MarketiAc' Butonuna Tıkla >= %60]
    H --> I[Kontrol 2: 'MarketTitle' Başlığını Doğrula >= %90]
    I --> J[Adım H: Boşluk Sayısı Kadar Yem Satın Al]
    J --> K[Yemleri Stackle ve Envantere Düzenle]
    K --> L[BuyCampfire Aktifse Kamp Ateşi Satın Al]
    L --> M[MarketExitButton ile Marketi Kapat]
```

### 📋 Balıkçı Adım Detayları:
1. **İhtiyaç Tespiti**: `InventoryBaitArea` içerisindeki boş slot sayısı hesaplanır (`emptyCount`).
2. **Balıkçı NPC Tespiti**: `FisherManSearchArea` alanında balıkçı aranır. Bulunamazsa fare ekran ortasında orta tuş basılı tutularak 20px sağa kaydırılır (kamera döndürülür, maks 18 deneme).
3. **Market Menüsü Açma**: Balıkçıya tıklanır, `OpenMarketPosition` alanındaki `MarketiAc` butonu doğrulanıp tıklanır.
4. **Market Doğrulama**: `MarketTitle` başlığının açıldığı teyit edilir.
5. **Satın Alma**: Marketteki yem simgesine `emptyCount` defa (2 sn aralıkla) ve `BuyCampfireEnabled` aktifse ilk 3 slotta tespit edilen boşluk sayısı kadar (`emptyFireCount` adet, 1 sn aralıkla) kamp ateşine sağ tıklanır.
6. **Düzenleme & Kapatma**: Satın alınan yemler ve kamp ateşleri birleştirilip envantere yerleştirilir, `MarketExitButton` tıklanarak market penceresi kapatılır.

---

## 3. Asıl Balık Tutma Döngüsü (`FishingExecutionFunction.cs`)

Her bir balık tutma döngüsünde aşağıdaki **6 adım** kesintisiz olarak yürütülür:

```mermaid
graph TD
    A[1. Envanter ve Slot Kontrolü] --> A1{Boş Slot Var mı?}
    A1 -- EmptySlot == 0 --► A2[1.1 Balık Öldürme Fonksiyonu]
    A2 --> A3[1.2 Yere Atma Fonksiyonu]
    A3 --> A4[1.3 Balık Pişirme Fonksiyonu]
    A4 --> A5{Pişirme Sonrası Boş Slot Açıldı mı?}
    A5 -- Hayır --► A6[🛑 Botu Durdur & MainForm Öne Getir]
    A5 -- Evet --► B[2. Yem Kontrolü ve Hazırlık]
    A1 -- Boş Slot > 0 --► B

    B --> B1{Yem Var mı?}
    B1 -- Yem Yok & BuyWorm==false --► B2[🛑 Botu Durdur / Uyarı Ver]
    B1 -- Yem Yok & BuyWorm==true --► B3[Balıkçı Bulma & Yem Alma]
    B3 --> B4{Yem Alındı mı?}
    B4 -- Hayır --► B2
    B4 -- Evet --► B5[Rastgele Bir Yeme Sağ Tıkla]
    B1 -- Yem Var --► B5

    B5 --> B6[Oltalama Hızı Beklemesi Min-Max ms]
    B6 --> C[3. Space Tuşuna Basarak Olta At]
    C --> C1[ChatBox Taraması Balık / AutoPass / Tutamazsın]
    C1 --> C2{Mesaj == Tutamazsın?}
    C2 -- Evet --► C3[🛑 Botu Durdur & Alan Uyarısı Göster]
    C2 -- Hayır --► D[4. Filtreleme ve Karar]

    D --> D1{Hedef Balığı Tut Aktif mi?}
    D1 -- Hayır / AutoPass --► D2[FishingMenuExitButton Tıkla]
    D2 --> D3[Animasyon İptali Yap]
    D3 --> A

    D1 -- Evet --► E[5. Balık Tutma Mini-Oyun]
    E --> E1[FishingMenuTitle Başlığını Bekle Timeout 15sn]
    E1 --> E2[Eşzamanlı: Mini-Oyun & Chat Waypoint Takibi]
    E2 --> E3[Animasyon İptali Yap]
    E3 --> F[6. Sonuç ve Döngü]

    F --> F1{Waypoint Sonucu?}
    F1 -- Tutamazsın --► C3
    F1 -- Balık Kaçtı / Diğer --► A
    F1 -- Balık Yakalandı --► F2[100ms Bekle]
    F2 --> A
```

### 📋 6 Adımlı Algoritma Açıklaması:

#### **1. Envanter ve Slot Kontrolü**
- `InventoryFishArea` taranarak `EmptySlot` şablonları sayılır (Deduplication / NMS ile).
- **Eğer `EmptySlot == 0` ise**: Envanterde yer kalmamıştır ➡️ **Balık Öldürme (`FishKillingFunction`)** ➔ **Yere Atma (`FishDropFunction`)** ➔ **Balık Pişirme (`FishCookingFunction`)** süreçleri kesintisiz sırayla işletilir.
  - Pişirme işleminin sonunda boş slot kontrolü yapılır. Boş slot açıldıysa (`EmptySlot > 0`) 2. adıma geçilir; açılamadıysa **Bot durdurulur** ve `MainForm` öne getirilir.
- **Eğer boş slot varsa (`EmptySlot > 0`)**: 2. adıma geçilir.

#### **2. Yem Kontrolü ve Hazırlık**
- `InventoryBaitArea` içerisindeki tüm yemler (`yem.png`, `yem200.png`) taranır.
- **Eğer yem yoksa**:
  - `BuyWormEnabled == false` ise: Bot durdurulur ve uyarı loglanır.
  - `BuyWormEnabled == true` ise: `StartupFishermanFunction.ExecuteAsync` çağrılarak balıkçıdan yem satın alınır. Satın alma başarısız olursa bot durdurulur.
- **Yem varsa**: Bulunan yemler arasından **rastgele bir tanesi** seçilip insansı hareketle sağ tıklanır.
- Fare envanter dışına çekilir.
- Ayarlanan **Oltalama Hızı** (`FishingSpeedMinMs` - `FishingSpeedMaxMs`) aralığında rastgele gecikme beklenir.

#### **3. Oltayı Fırlatma ve İlk Kontroller**
- **Space** tuşuna basılarak olta suya atılır (80ms basılı tutma).
- `ChatBoxPosition` alanı taranır (`FishNames.All`, `AutoPass.All`, `Waypoints.Tutamazsin`).
- **Eğer "Tutamazsın" mesajı geldiyse**: Bot derhal durdurulur, `MainForm` öne getirilir ve ekranda alan uyarı mesajı gösterilir.

#### **4. Filtreleme ve Karar**
- ChatBox'ta tespit edilen balık adı kullanıcının `FishFilter` ayarlarıyla karşılaştırılır:
  - **Eğer "Balığı Tut" / "Yakala" KAPALI ise veya `AutoPass` şablonuysa**:
    1. `FishingMenuTitle` beklenir.
    2. `FishingMenuExitButtonPosition` alanındaki çıkış butonuna tıklanarak menü kapatılır.
    3. Binek/zırh ile Animasyon İptali (`Ctrl + G`) yapılır.
    4. **Doğrudan 1. Adıma geri dönülür.**
  - **Eğer "Balığı Tut" / "Yakala" AÇIK ise**: 5. Adıma geçilir.

#### **5. Balık Tutma (Mini-Oyun)**
- `FisherManSearchArea` içerisinde `FishingMenuTitle` başlığı beklenir (**15 saniye zaman aşımı**).
- Başlık göründüğü anda eşzamanlı iki görev başlatılır:
  1. `FishingMinigameFunction.ExecuteMinigameAsync` (Pembe halka & balık hedefi tıklama)
  2. `WatchWaypointsAsync` (Chat alanındaki sonuç waypoint'lerini izleme)
- Waypoint geldiğinde veya mini-oyun bittiğinde **Animasyon İptali** (`Ctrl + G`) yapılır.

#### **6. Sonuç ve Döngü**
- Chat alanında tespit edilen Waypoint kontrol edilir:
  - `Tutamazsin`: Bot durdurulur ve uyarı gösterilir.
  - `YakalananBalik`: **100 ms beklenir** ve **1. Adıma dönülür** (1. Adım otomatik olarak yeni boş slot sayısını kontrol eder).
  - `Balık Kaçtı` veya diğer durumlar: **Doğrudan 1. Adıma dönülür.**

---

## 4. Balık Yakalama Mini-Oyunu (`FishingMinigameFunction.cs`)

Balık tutma penceresi açıldığında balığı yakalamak için çalışan yüksek performanslı tıklama mekanizması:

```mermaid
graph TD
    A[Mini-Oyun Başladı] --> B[Pembe Halka Kontrolü CircleColorControlArea1-4]
    B --> C{Pembe Renk #FFADC7 Var mı?}
    C -- Hayır --► B
    C -- Evet --► D[Balık Hedefi Tespiti FishCircleArea 17 Renk]
    D --> E{Balık Pikselleri Bulundu mu?}
    E -- Evet --► F[Merkez Piksele Donanımsal Sol Tık Bas]
    E -- Hayır --► G[Varsayılan Merkez Noktaya Tıkla]
    F --> H{Halka 100ms Boyunca Kesintisiz Pembe mi?}
    H -- Evet --► I[İkinci Tıklama Hakkını Kullan]
    H -- Hayır --► J[Halkanın Tekrar Pembeye Dönmesini Bekle]
    J --> B
```

### 📋 Mini-Oyun Kuralları:
1. **Pembe Halka Taraması**: `CircleColorControlArea1`, `2`, `3`, `4` bölgelerinde `#FFADC7` rengi taranır. Renk görüldüğünde balık çemberin içindedir.
2. **Balık Hedefi Tespiti**: `FishCircleArea` içerisinde 17 farklı balık renk değeri taranarak kümelenmiş hedef balık bulunur.
3. **Ultra Hızlı Tıklama**: Hedef balık bulunduğu anda fare gecikmesiz olarak hedefin üzerine taşınır ve sol tık basılır.
4. **100ms Sürekli Pembe Kuralı**: Çember kesintisiz olarak 100ms boyunca pembe kalırsa ikinci bir tıklama hakkı tanınır.

---

## 5. Animasyon İptali Mekanizması (`PerformAnimationCancelAsync`)

Balık tutma veya pas geçme işlemi tamamlandığında karakterin oltayı sudan çekme bekleme animasyonunu iptal ederek tur başına **~2.5 saniye** kazandırır:

* **`Binek Kullan (mount)` Modu (Varsayılan)**:
  - Oltalama hızı aralığında hesaplanan rastgele gecikmeyle **2 kez ardışık donanımsal `Ctrl + G`** kombinasyonu basılır.
* **`Zırh Değiştir (armor)` Modu**:
  - Envanterin ilk slotuna (zırha) insansı sağ tık yapılarak zırh çıkarılıp takılır.

---

## 6. Balık Öldürme Süreci (`FishKillingFunction.cs`)

Envanter balık alanı (`InventoryFishArea`) dolduğunda (`EmptySlot == 0`), yere atma ve pişirme süreçlerinden hemen önce çalıştırılır:

```mermaid
graph TD
    A[Adım A: Slotlarda Fareyi Gezdir] --> B[Adım B: Öldürülebilir Canlı Balıkları Tara]
    B --> C{En Yüksek Eşleşme FishIconTemplates & 'Öldür' Seçili mi?}
    C -- Hayır --► D[Öldürülecek Balık Yok ➔ Yere Atmaya Geç]
    C -- Evet --► E[Öldürülecekler Listesine Ekle]
    E --> F[Adım C: Öldürülebilir Balıklara Sırayla Sağ Tıkla]
    F --> G[Fareyi Envanter Dışına Çek]
    G --> H[Doğrudan Yere Atma Adımına Geç]
```

### 📋 Öldürme Algoritması Adımları:
- **Adım A**: Tüm slotlarda fareyi gezdir (`HoverAcrossInventoryFishAreaAsync`).
- **Adım B**: `InventoryFishArea` alanında `GrilledFishes`, `DeadFishes` ve `FishIconTemplates` (`Common`, `Rare`) şablonları ile eşzamanlı template matching yapılır.
  - Non-Maximum Suppression (NMS) uygulanarak aynı pozisyondaki en yüksek benzerlik skoruna sahip şablon bulunur.
  - Eğer en yüksek eşleşme **`FishIconTemplates`** (yani `Izgara_` veya `Ölü_` ile **başlamayan** normal canlı balık) ise ve kullanıcının `FishFilter` ayarlarında bu balık için **"Öldür"** seçeneği işaretliyse, balık öldürülecekler listesine eklenir.
- **Adım C**: Listeye alınan tüm öldürülebilir balıklara sırayla birer kez donanımsal/insansı sağ tıklanır (200-350ms bekleme ile). İşlem bitiminde fare envanter dışına çekilir ve **doğrudan Yere Atma sürecine (`FishDropFunction`)** geçilir.

---

## 7. Yere Atma Süreci (`FishDropFunction.cs`)

Öldürme süreci tamamlandıktan sonra, pişirme işleminden önce çalıştırılır:

```mermaid
graph TD
    A[Adım A: Slotlarda Fareyi Gezdir] --> B[Adım B: Yere Atılacak Öğeleri Tara]
    B --> C{En Yüksek Eşleşen Şablon & 'Yere At' Seçili mi?}
    C -- Hayır --► D[Yere Atılacak Öğe Yok ➔ Pişirmeye Geç]
    C -- Evet --► E[Yere Atılacaklar Listesine Ekle]
    E --> F[Adım C1: Öğeyi Envanter Sol Dışına Sürükle ve Bırak]
    F --> G[Adım C2: 50ms Bekle ve Onay Kutusunu Tara DropItemQuestionArea]
    G --> H[Adım C3: DropItemQuestionYesButton Butonuna Tıkla]
    H --> I{Tüm Öğeler Bitti mi?}
    I -- Hayır --► F
    I -- Evet --► J[Adım D: Fareyi Dışarı Çek & Pişirme Adımına Geç]
```

### 📋 Yere Atma Algoritması Adımları:
- **Adım A**: Tüm slotlarda fareyi gezdir (`HoverAcrossInventoryFishAreaAsync`).
- **Adım B**: `InventoryFishArea` alanında `GrilledFishes`, `DeadFishes`, `FishIconTemplates`, `Others` ve `DeadFishLoot` şablonları taranır (NMS).
  - **Kritik Kural**: **Izgara Balıklar (`Izgara_`) KESİNLİKLE yere atılmaz.**
  - En yüksek eşleşen şablon Izgara **olmayan** bir öğe ise ve kullanıcının ayarlarında **"Yere At"** seçeneği işaretliyse listeye alınır.
- **Adım C1**: Yere atılacak öğeler sırayla tutulur ve `InventoryPosition` bölgesinin dışına — solunda 100px mesafede rastgele bir alana sürüklenip bırakılır (`Drag & Drop`).
- **Adım C2 - C3 (Onay & Teyit Döngüsü)**: Bırakıldıktan sonra `DropItemQuestionArea` içerisinde `DropItemQuestionYesButton` butonuna tıklanır. Tıkladıktan sonra **50ms beklenip `DropItemQuestion` şablonu tekrar taranır**. Soru kutusu hala ekrandaysa tekrar tıklanır; pencere kapandığında döngüden çıkılarak sıradaki öğeye geçilir.
- **Adım D**: Tüm yere atılacak öğeler bittiğinde fare envanter dışına çekilir ve **doğrudan Pişirme sürecine (`FishCookingFunction`)** geçilir.

---

## 8. Balık Pişirme Süreci (`FishCookingFunction.cs`)

Yere atma süreci tamamlandıktan sonra devreye girer ve aşağıdaki algoritmayı işletir:

```mermaid
graph TD
    A[Adım A: Slotlarda Fareyi Gezdir] --> B[Adım B: Pişirilebilir Balıkları Tara]
    B --> C{En Yüksek Eşleşme Izgara_ Değil & 'Pişir' Seçili mi?}
    C -- Hayır --► D4[Adım D4: Boş Slot Kontrolü]
    C -- Evet --► C1[Adım C1: Çantada Kamp Ateşi Var mı?]
    
    C1 -- Hayır & BuyCampfire==false --► D4
    C1 -- Hayır & BuyCampfire==true --► C4[Adım C4: Balıkçıdan Kamp Ateşi Satın Al]
    C4 --> C1
    
    C1 -- Evet --► D[Adım D: Kamp Ateşine Sağ Tıkla ve Yak]
    D --> D2[Adım D2: Yerdeki Kamp Ateşini Tespiti FisherManSearchArea]
    D2 --> D3[Adım D3: Balıkları Sırayla Ateşe Sürükle ve Bırak]
    D3 --> D3Check{Her Sürükleme Öncesi Yerde Ateş Var mı?}
    D3Check -- Ateş Söndü --► C1
    D3Check -- Pişirme Bitti --► D4
    
    D4 --> D4Check{Pişirme Sonrası Boş Slot Açıldı mı?}
    D4Check -- Boş Slot > 0 --► E[Adım E: Balık Tutma Döngüsüne Devam Et]
    D4Check -- Boş Slot == 0 --► F[🛑 Botu Durdur & MainForm Öne Getir]
```

### 📋 Pişirme Algoritması Adımları:
- **Adım A**: Tüm slotlarda fareyi gezdir (`HoverAcrossInventoryFishAreaAsync`).
- **Adım B**: `InventoryFishArea` alanında `GrilledFishes`, `DeadFishes` ve `FishIconTemplates` şablonları taranır (NMS).
  - En yüksek eşleşmesi `Izgara_` **olmayan** (yani `DeadFishes` veya `FishIconTemplates`) ve ayarlarında **"Pişir"** seçeneği işaretli olan balıklar listeye alınır.
- **Adım C1**: Çantada (`InventoryBaitArea`) kamp ateşi (`ates.png`) var mı kontrol edilir.
- **Adım C2 - C3**: Kamp ateşi yoksa ve `BuyCampfireEnabled == false` ise `InventoryFishArea` boş slotları sayılır. Boşluk yoksa bot durdurulur; boşluk varsa döngüye devam edilir.
- **Adım C4**: Kamp ateşi yoksa ve `BuyCampfireEnabled == true` ise balıkçıdan kamp ateşi satın alınır.
- **Adım D**: Çantadaki kamp ateşlerinden rastgele 1 tanesine sağ tıklanarak yakılır.
- **Adım D2**: `FisherManSearchArea` içerisinde `KampAtesiFloor` ve `KampAtesiFloor2` aranır (>= %60) ve konum koordinatları alınır.
- **Adım D3**: Pişirilecek tüm balıklar sırayla yerdeki kamp ateşine sürüklenip bırakılır (`Drag & Drop`).
  - **ÖNEMLİ**: Her sürükleme öncesinde yerde ateşin varlığı teyit edilir. Ateş söndüyse **C1 adımına geri dönülür**.
- **Adım D4**: Pişirme sonrasında boş slotlar sayılır:
  - **Evet (`EmptySlot > 0`)**: **Adım E**'ye geçilir ve balık tutma döngüsüne devam edilir.
  - **Hayır (`EmptySlot == 0`)**: **Bot durdurulur** ve `MainForm` öne getirilir.
- **Adım E**: Tüm balıklar piştiğinde ve boş slot açıldığında balık tutma döngüsüne geri dönülür.

---

## 9. Eşya ve Balık Eylem Uyumluluk Tablosu (Öldür / Yere At / Pişir)

Aşağıdaki tablo, envanterde bulunan tüm nesnelerin bot tarafından hangi adımlarda işlenebileceğini gösterir:

| Kategori / Öğe Grubu | Örnekler / Şablonlar | ⚔️ Öldürülebilir mi? | 🗑️ Yere Atılabilir mi? | 🔥 Pişirilebilir mi? |
| :--- | :--- | :---: | :---: | :---: |
| **Canlı Yaygın Balıklar (Common)** | Büyük Sudak, Dere Alabalığı, Gökkuşağı Alabalığı, Hamsi, Levrek, Lüfer, Nehir Alabalığı, Ot Sazanı, Ringa, Sazan, Som, Sudak, Tekir, Yayın, Zargana | ✅ **EVET** *(Sağ Tık)* | ✅ **EVET** *(Sürükle-Bırak)* | ✅ **EVET** *(Ateşe Sürükle)* |
| **Canlı Nadir Balıklar (Rare)** | Altın Sudak, Aynalı Sazan, Kadife Balığı, Kral Yengeci, Kurbağa Balığı, Palamut, Sevimli Balık, Yabbie Yengeci, Yılan Başı Balığı | ✅ **EVET** *(Sağ Tık)* | ✅ **EVET** *(Sürükle-Bırak)* | ✅ **EVET** *(Ateşe Sürükle)* |
| **Ölü Balıklar (DeadFishes)** | Ölü Büyük Sudak, Ölü Hamsi, Ölü Lüfer, Ölü Palamut, Ölü Altın Sudak vb. (24 Tür) | ❌ **HAYIR** *(Zaten Ölü)* | ✅ **EVET** *(Sürükle-Bırak)* | ✅ **EVET** *(Ateşe Sürükle)* |
| **Izgara Balıklar (GrilledFishes)** | Izgara Büyük Sudak, Izgara Hamsi, Izgara Lüfer vb. (24 Tür) | ❌ **HAYIR** | 🛑 **KESİNLİKLE HAYIR** *(Korumalı)* | ❌ **HAYIR** *(Zaten Pişmiş)* |
| **Diğer Nesneler & Saç Boyaları (Others)** | Altın Anahtar, Gümüş Anahtar, Altın Yüzük, Lucy'nin Yüzüğü, Saç Boyaları, Bilge Kralın Eldiveni / Sembolü, Görünmezlik Pelerini | ❌ **HAYIR** | ✅ **EVET** *(Sürükle-Bırak)* | ❌ **HAYIR** |
| **Ölü Balık Ganimetleri (DeadFishLoot)** | Beyaz İnci, Mavi İnci, Kankırmızı İnci, İstiridye, Taş Parçası | ❌ **HAYIR** | ✅ **EVET** *(Sürükle-Bırak)* | ❌ **HAYIR** |

---

## 10. Önemli Yardımcı Fonksiyonlar ve Kısayollar

### 🖱️ Envanter Gezdirme Fonksiyonu (`HoverAcrossInventoryFishAreaAsync`)
- `InventoryFishArea` içerisindeki 5 sütun x 7 satır (35 slot) üzerinde fareyi yukarıdan aşağıya ve aşağıdan yukarıya zikzak şeklinde gezdirir.
- Fonksiyon [`FishingExecutionFunction.cs`](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Functions/FishingExecutionFunction.cs) içerisinde hazır ve modüler olarak bekletilmektedir.

### ⌨️ Global Kısayol Tuşları:
- **`F1`**: **Acil Durdurma (Emergency Stop)** ➡️ Çalışan tüm istemci botlarını anında durdurur ve ana formu öne getirir.
- **`F2`**: **Toplu Başlatma (Start All)** ➡️ HWND oyun penceresi bağlanmış tüm istemcileri aynı anda başlatır.

### 🛑 'Tutamazsin' Alan Güvenliği:
- Yanlış veya balık tutulamayan bir bölgede olta atıldığında chat'te çıkan `tutamazsin.png` şablonu anında yakalanır, bot durdurulur ve kullanıcıya açılır pencere ile bildirim verilir.

### 📝 Log Konsolu (`BotLogger.cs` & `FishBotPage.cs`):
- İstemci bazlı durumlar, balık tespitleri ve hata logları renk kodlarıyla (Mavi: Bilgi, Yeşil: Başarı, Sarı: Uyarı, Kırmızı: Hata) `logPanel` konsolunda anlık gösterilir. Konsol her 15 logda bir otomatik temizlenerek bellek ve performans optimize edilir.
