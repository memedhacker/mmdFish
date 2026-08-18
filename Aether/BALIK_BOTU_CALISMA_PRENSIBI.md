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

Her bir balık tutma döngüsünde aşağıdaki adımlar kesintisiz olarak yürütülür:

```mermaid
graph TD
    A[1. Envanterdeki Yemleri Tara] --> B[2. Rastgele Bir Yeme Sağ Tıkla]
    B --> B1{2.1 Envanter Boş Slot Kontrolü}
    B1 -- EmptySlot == 0 --► B2[Öldürme ve Pişirme Süreçlerini Çalıştır]
    B2 -- Boş Slot Açılamadı --► B3[🛑 Botu Durdur & MainForm Öne Getir]
    B2 -- Boş Slot Açıldı --► C[Oltalama Hızı Beklemesi Min-Max ms]
    B1 -- Boş Slot Var --► C
    C --> D[3. Space Tuşuna Basarak Olta At]
    D --> E[4. ChatBox Taraması Balık Adı / AutoPass / Tutamazsin]
    E --> F{5. Filtre Kontrolü: Balığı Tut Aktif mi?}
    F -- Hayır / AutoPass --► G[FishingMenuExitButton Tıkla]
    G --> H[Animasyon İptali Yap]
    H --> A
    F -- Evet --► I[6. FishingMenuTitle Başlığını Bekle]
    I --> J[7. Eşzamanlı: Mini-Oyun & Chat Waypoint Takibi]
    J --> K[Animasyon İptali Yap]
    K --> L{8. Waypoint Kontrolü}
    L -- Tutamazsin --► LQ[🛑 Botu Durdur & Alan Uyarısı Göster]
    L -- Diğer / Kaçtı --► A
    L -- YakalananBalik --► M[100ms Bekle & EmptySlot Sayısını Tara]
    M --> N{EmptySlot == 0?}
    N -- Hayır --► A
    N -- Evet --► O[Öldürme ve Pişirme Süreçlerini Çalıştır]
    O --> P{Boş Slot Açıldı mı?}
    P -- Evet --► A
    P -- Hayır --► Q[🛑 Botu Durdur & MainForm Öne Getir]
```

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

## 7. Balık Öldürme Süreci (`FishKillingFunction.cs`)

Envanter balık alanı (`InventoryFishArea`) dolduğunda (veya başlangıçta çanta doluysa), pişirme işleminden hemen önce çalıştırılır:

* **Başlangıç (5x7 Slot Gezme)**: İşlem başında fare 5x7 (35 slot) ızgara boyunca yukarı-aşağı gezdirilir (`HoverAcrossInventoryFishAreaAsync`).
* **Adım A (Tüm Şablonlarla Tarama & Canlı Balık Ayrımı)**: `InventoryFishArea` alanı **TÜM balık şablonları** (Normal, Ölü, Izgara) ile taranır. Non-Maximum Suppression (NMS) ile her slotun en yüksek benzerlikteki şablonu belirlenir.
  - Sadece en yüksek eşleşmesi `Izgara_` veya `Ölü_` **olmayan** (yani kesin olarak Normal Canlı) ve ayarlarında "Öldür" işaretli olan balıklar listeye alınır.
* **Adım B (Sağ Tıklama ile Öldürme)**: Öldürülmeye uygun balıklara sırayla sağ tıklanır.
* **Adım C (Uygun Balık Yoksa)**: Eğer envanterde öldürülecek canlı balık yoksa doğrudan pişirme adımına geçilir.
* **Adım E (5x7 Slot Gezme ve Pişirmeye Geçiş)**: Tüm balıklar öldüğünde fare 5x7 (35 slot) ızgara boyunca tekrar yukarı-aşağı gezdirilir (`HoverAcrossInventoryFishAreaAsync`), fare sol dışarı çekilir ve ardından pişirme adımına geçilir.

---

## 8. Balık Pişirme ve Envanter Yönetimi (`FishCookingFunction.cs`)

Envanter balık alanı (`InventoryFishArea`) dolduğunda veya pişirme tetiklendiğinde aşağıdaki adımlar sırayla yürütülür:

* **Adım A (Tüm Şablonlarla Tarama & Pişirilebilir Balık Ayrımı)**: `InventoryFishArea` alanı **TÜM balık şablonları** (Normal, Ölü, Izgara) ile taranır. NMS ile her slotun en yüksek benzerlikteki şablonu belirlenir.
  - En yüksek eşleşmesi `Izgara_` **olmayan** (yani `Ölü_` veya `Normal`) ve ayarlarında "Pişir" işaretli olan balıklar pişirme listesine eklenir.
* **Adım B2 (Uygun Balık Yoksa)**: Eğer `InventoryFishArea` içerisinde pişirilecek uygun balık yoksa bot durdurulur ve ana form öne getirilir.
* **Adım B (Kamp Ateşi Kurulumu)**: Pişirilmeye uygun balık(lar) varsa `InventoryBaitArea` içerisinden herhangi bir `ates.png` şablonuna sağ tıklanır, **100ms beklenir** ve fare envanter dışına çekilir.
* **Adım C (Zemin Ateşi Tespiti)**: `FisherManSearchArea` bölgesinde `KampAtesiFloor` veya `KampAtesiFloor2` şablonları (**>= %60**) aranır ve konum koordinatları alınır.
* **Adım D (Ateşe Sürükle ve Bırak)**: Pişirilmeye uygun balıklar sırayla yerdeki kamp ateşinin koordinatına sürüklenip bırakılır (`Drag & Drop`).
* **Adım E (Boş Alan Kontrolü ve Döngü Kararı)**: Tüm balıklar piştiğinde `InventoryFishArea` alanındaki boş slot sayısı tekrar taranır. Boş yer açılmışsa (`EmptySlot > 0`) balık tutma döngüsüne devam edilir; açılamamışsa bot durdurulur.

---

## 8. 'Tutamazsin' Waypoint Tespiti ve Alan Uyarısı

Eğer karakter uygun olmayan bir konumda olta atarsa veya sohbet alanında `tutamazsin.png` şablonu tespit edilirse:
1. **Güvenli Durdurma**: `FishBotService.Instance.StopFishBot(clientId)` çağrılarak bot anında durdurulur.
2. **Pencereyi Öne Getirme**: `MainForm` simge durumundan çıkarılarak ekranın önüne odaklanır.
3. **Bildirim Penceresi**: Ekranda `"Client #X balık tutabilecek bir alanda değil. Tüm Bot İşlevleri durduruldu."` uyarısını içeren bir `MessageBox` penceresi gösterilir.

