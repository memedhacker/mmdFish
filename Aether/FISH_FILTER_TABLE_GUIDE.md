# 🎣 FishBot Balık Filtre Tablosu Geliştirme Dokümanı

Bu doküman, **FishBotPage** arayüzünde balık filtresinin altında dinamik olarak oluşturulan balık ve diğer öğelerin yönetim tablolarının yapısını ve teknik detaylarını açıklamaktadır.

---

## 🗂️ 1. İkon ve Klasör Yapısı

Balık ve diğer ikonlar aşağıdaki dizinlerde kategorize edilmiştir:
- **`Assets/FishIcons/rare/`**: Nadir balıklara ait `.png` ikonları.
- **`Assets/FishIcons/common/`**: Yaygın balıklara ait `.png` ikonları.
- **`Assets/FishIcons/others/`**: Balık dışındaki eşyalara (Anahtar, Saç Boyası, Pelerin vb.) ait `.png` ikonları.
- **`Assets/FishIcons/others/deadFishLoot/`**: Ölü balık kırıldığında/açıldığında elde edilen eşyalara (İnci, İstiridye, Taş Parçası) ait `.png` ikonları.

### İsimlerin Formatlanması
Dosya isimlerindeki alt tire (`_`) karakterleri temizlenerek her kelimenin ilk harfi büyük, kalan harfleri küçük olacak şekilde düzeltilmiştir (Örn: `Beyaz_İnci.png` ➔ `Beyaz İnci`).

---

## 📊 2. Tablo Tasarımları ve Sütun Yapıları

### A. Rare ve Common Balık Tabloları (4 Sütun)
1. **İkon & Balık Adı** (Sol): Balığın ikonu ve ismi.
2. **Balığı Tut** (Yeşil Checkbox): `Checked = true` (Varsayılan)
3. **Pişir** (Turuncu Checkbox): `Checked = false`
4. **Öldür** (Açık Mavi Checkbox): `Checked = false`
5. **Yere At** (Pembe Checkbox): `Checked = false`

### B. Diğer Öğeler (Others) Tablosu (2 Sütun)
1. **İkon & Öğe Adı** (Sol): Öğenin ikonu ve ismi.
2. **Yakala** (Yeşil Checkbox): `Checked = true` (Varsayılan)
3. **Yere At** (Pembe Checkbox): `Checked = false`

### C. Ölü Balık Ganimetleri (Dead Fish Loot) Tablosu (1 Sütun)
Tabloların en altında **"Ölü Balık Ganimetleri (Dead Fish Loot)"** başlığı ile konumlandırılmıştır.
1. **İkon & Öğe Adı** (Sol): Öğenin ikonu ve ismi (`İstiridye`, `Beyaz İnci`, `Mavi İnci`, `Kankırmızı İnci`, `Taş Parçası`).
2. **Yere At** (Pembe Checkbox): Öğenin yere atılıp atılmayacağını belirler. (Varsayılan: `Checked = false`)

---

## 💻 3. İlgili Kod Dosyaları ve Değişiklikler

- **[FishBotPage.Designer.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Pages/FishBotPage.Designer.cs)**:
  - `fishFilterPanel` (`Sunny.UI.UIPanel`) dinamik alan olarak konumlandırıldı.

- **[FishBotPage.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Pages/FishBotPage.cs)**:
  - `BuildFishFilterTable()`, `CreateFishCategoryTable()`, `CreateOthersCategoryTable()` ve `CreateSingleOptionCategoryTable()` metodlarıyla 4 farklı tablo dinamik olarak yüklenir ve boyutlandırılır.
