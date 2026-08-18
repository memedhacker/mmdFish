# 🎣 Aether Balık Botu — Detaylı Çalışma Prensibi ve Akış Şeması

Bu doküman, **Aether Metin2 Balık Botu**'nun başlatma sekansından balık tutma, mini-oyun yönetimi, animasyon iptali, balıkçı etkileşimi ve kamp ateşinde balık pişirme mekanizmalarına kadar tüm adımlarını detaylandırmaktadır.

---

## 📑 İçindekiler
1. [Genel Mimari ve Kısayollar (F1 / F2)](#1-genel-mimari-ve-kısayollar)
2. [Başlangıç Hazırlık Sekansı (Startup Sequence)](#2-başlangıç-hazırlık-sekansı)
3. [Balıkçı NPC ve Market Yönetimi](#3-balıkçı-npc-ve-market-yönetimi)
4. [Asıl Balık Tutma Döngüsü (Fishing Cycle)](#4-asıl-balık-tutma-döngüsü)
5. [Balık Yakalama Mini-Oyunu (Minigame Logic)](#5-balık-yakalama-mini-oyunu)
6. [Animasyon İptali (Animation Cancel)](#6-animasyon-iptali)
7. [Balık Pişirme ve Envanter Yönetimi (Fish Cooking)](#7-balık-pişirme-ve-envanter-yönetimi)

---

## 1. Genel Mimari ve Kısayollar

| Kısayol / Buton | Görevi |
| :--- | :--- |
| **`F2` / Başlat Butonu** | HWND bağlı olan tüm istemcileri (veya seçili istemciyi) başlatır. Oyun pencerelerini ekranın en önüne odaklar (`BringWindowToFront`) ve ana uygulama penceresini simge durumuna küçültür (`Minimize`). |
| **`F1` / Durdur Butonu** | **Acil Durdurma (Emergency Stop):** Çalışan tüm istemcileri anında durdurur ve ana uygulama penceresini simge durumundan çıkararak ekranın en önüne getirir (`Restore & Focus`). |
| **Log Konsolu** | Arayüz performansını ve akıcılığını korumak adına her 15 log satırında bir otomatik olarak temizlenir. |

---

## 2. Başlangıç Hazırlık Sekansı (`FishBotStartupFunction.cs`)
Bot ilk başlatıldığında **yalnızca 1 kez** aşağıdaki adımları sırasıyla yürütür:

1. **Pencere Odaklama**: İstemci penceresi ekranın en önüne getirilir.
2. **Merkeze Odaklama Tıklaması**: Oyun penceresinin tam ortasına insansı kavisle gidilip 1 kere sağ tıklanır (`Right Click`).
3. **Kamera Açısı Ayarı**:
   - `F` tuşuna donanımsal olarak **3 saniye** basılı tutulur.
   - `G` tuşuna donanımsal olarak **3 saniye** basılı tutulur.
4. **Ekipman Menüsü Kapatma**:
   - `EquipmentMenuTitle` şablonu taranır; bulunamazsa `I` tuşuna basılarak menü açılır/kapanır.
   - Menü tespit edildiğinde `EquipmentMenuExitButton` butonuna insansı kavisle tıklanarak menü kapatılır.
5. **Envanter Sayfası Kontrolü**:
   - `InventoryPagesPosition` alanı taranır. İstemci ayarlarındaki hedef sayfa (`InventoryPage`: Sayfa 1, 2, 3, 4) açık değilse o sayfaya tıklanarak açılır.
6. **Yemleri Birleştirme (Stackleme)**:
   - Envanterdeki tüm yemler (`yem.png`, `yem200.png`) taranıp üst üste sürüklenerek birleştirilir.
7. **Kamp Ateşlerinin Düzenlenmesi**:
   - Envanterdeki kamp ateşleri (`ates.png`) taranarak ilk 3 slota düzenlenir (3'ten fazlaysa üst üste istiflenir).
8. **Yemlerin Taşınması**:
   - Yemler/solucanlar envanterin 4. slotu ve sonrasındaki boşluklara taşınır.
9. **Balıkçı Etkileşimi (Opsiyonel)**:
   - `BuyWorm` veya `BuyCampfire` ayarı aktifse balıkçıdan alışveriş adımı işletilir.
10. **Başlangıç Envanter Doluluk & Pişirme Kontrolü**:
    - `InventoryFishArea` alanındaki boş slotlar (`EmptySlot`) sayılır.
    - Eğer hiç boş slot yoksa (`EmptySlot == 0`), otomatik olarak **Balık Pişirme** süreci tetiklenir. Boş slot açılamazsa bot durdurulur.

---

## 3. Balıkçı NPC ve Market Yönetimi (`StartupFishermanFunction.cs`)
Eğer `BuyWormEnabled` veya `BuyCampfireEnabled` ayarları açıksa:

```
[Adım A] InventoryBaitArea Kontrolü (Boşluk var mı?)
   │
   ├─► Dolu ise: Market açılmaz, doğrudan balık tutmaya geçilir.
   │
   └─► Boş yer varsa:
         │
         ├─► [Adım B-E] Balıkçı NPC Aranır (FisherManSearchArea)
         │      └─► Bulunamazsa: Fare ekran ortasında tekerlek (orta tuş) basılı tutularak 20px sağa kaydırılır.
         │
         ├─► [Adım F] Balıkçıya Tıklanır (1 sn beklenir)
         │
         ├─► [Kontrol 1] 'MarketiAc' butonu doğrulanır (>= %60) ve tıklanır.
         │
         ├─► [Kontrol 2] 'MarketTitle' başlığı doğrulanır (>= %90).
         │
         ├─► Yem ve Kamp Ateşleri boş slot sayısı kadar satın alınır ve envantere düzenlenir.
         │
         └─► 'MarketExitButton' tıklanarak market penceresi kapatılır.
```

---

## 4. Asıl Balık Tutma Döngüsü (`FishingExecutionFunction.cs`)

Her bir balık tutma döngüsünde aşağıdaki 6 adım kesintisiz olarak yürütülür:

```mermaid
graph TD
    A[1. Envanter ve Slot Kontrolü] --> A1{Boş Slot Var mı?}
    A1 -- EmptySlot == 0 --► A2[🛑 Botu Durdur & MainForm Öne Getir]
    A1 -- Boş Slot > 0 --► B[2. Yem Kontrolü ve Hazırlık]

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

### 📋 Adım Detayları:

1. **Envanter ve Slot Kontrolü**:
   - Envanterdeki boş slot sayısı taranır (`InventoryFishArea` içerisindeki `EmptySlot`).
   - Eğer boş slot yoksa (`EmptySlot == 0`): Bot durdurulur ve MainForm öne getirilir.
   - Boş slot varsa 2. adıma geçilir.
2. **Yem Kontrolü ve Hazırlık**:
   - Envanterdeki yemler taranır. Yem yoksa ve `BuyWorm == false` ise bot durdurulur; `BuyWorm == true` ise balıkçıdan yem alınır.
   - Yem varsa rastgele bir yeme sağ tıklanır, fare dışarı çekilir ve oltalama hızı süresince beklenir.
3. **Oltayı Fırlatma ve İlk Kontroller**:
   - Space tuşuna basılarak olta atılır.
   - ChatBox taranır (Balık Adı / AutoPass / Tutamazsın). "Tutamazsın" mesajı geldiyse bot durdurulur ve alan uyarısı gösterilir.
4. **Filtreleme ve Karar**:
   - Filtre kontrolü yapılır: Hedef balık için "Balığı Tut" / "Yakala" aktif mi?
   - Hayır veya AutoPass ise: `FishingMenuTitle` beklenir, `FishingMenuExitButton` tıklanır, animasyon iptali (`Ctrl+G`) yapılır ve 1. adıma dönülür.
5. **Balık Tutma (Mini-Oyun)**:
   - Evet ise: `FishingMenuTitle` başlığı beklenir (15 sn zaman aşımı).
   - Eşzamanlı olarak Mini-Oyun (`FishingMinigameFunction`) ve Chat Waypoint takibi yürütülür.
   - Bitiminde binek animasyon iptali (`Ctrl+G`) yapılır.
6. **Sonuç ve Döngü**:
   - Balık kaçtı veya diğer durumlarda doğrudan 1. adıma dönülür.
   - Balık yakalandığında (`YakalananBalik`) 100 ms beklenir ve 1. adıma dönülür (1. adım envanter boşluğunu kontrol eder).

---

## 5. Balık Yakalama Mini-Oyunu (`FishingMinigameFunction.cs`)

1. **Pembe Halka Taraması**:
   - `CircleColorControlArea1`, `2`, `3`, `4` bölgelerinde `#FFADC7` rengi taranır.
   - Pembe renk tespit edildiğinde balık çemberin içine girmiş demektir.
2. **Balık Hedefi Tespiti**:
   - `FishCircleArea` içerisinde 17 farklı balık renk değeri taranır.
3. **Ultra Hızlı ve Doğrulanmış Tıklama**:
   - Hedef balık pikseli bulunduğu anda fare **gecikmesiz ve doğrudan** hedefin koordinatına taşınır.
   - Tıklama öncesinde farenin tam hedefin üzerinde olduğu doğrulanır ve donanımsal sol tık basılır.
4. **100ms Sürekli Pembe Kuralı**:
   - Halka her pembeye döndüğünde 1 tıklama yapılır.
   - Eğer çember kesintisiz olarak **100ms boyunca pembe kalmaya devam ederse** ikinci bir tıklama hakkı tanınır.

---

## 6. Animasyon İptali (`PerformAnimationCancelAsync`)

Balık tutma bittiğinde karakterin oltayı sudan çekme animasyonunu iptal ederek zamandan tasarruf sağlar:

* **`Binek Kullan (mount)` Modu (Varsayılan)**:
  - Oltalama hızı aralığında hesaplanan rastgele gecikmeyle **2 kez ardışık donanımsal `Ctrl + G`** kombinasyonu basılır.
* **`Zırh Değiştir (armor)` Modu**:
  - Envanterin ilk slotuna (zırha) insansı sağ tık yapılarak zırh çıkarılıp takılır.

---

## 7. 'Tutamazsin' Waypoint Tespiti ve Alan Uyarısı

Eğer karakter uygun olmayan bir konumda olta atarsa veya sohbet alanında `tutamazsin.png` şablonu tespit edilirse:
1. **Güvenli Durdurma**: `FishBotService.Instance.StopFishBot(clientId)` çağrılarak bot anında durdurulur.
2. **Pencereyi Öne Getirme**: `MainForm` simge durumundan çıkarılarak ekranın önüne odaklanır.
3. **Bildirim Penceresi**: Ekranda `"Client #X balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruldu."` uyarısını içeren bir `MessageBox` penceresi gösterilir.

