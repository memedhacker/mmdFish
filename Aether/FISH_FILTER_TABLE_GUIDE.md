# 🎣 FishBot Balık Filtre Tablosu Geliştirme Dokümanı

Bu doküman, **FishBotPage** arayüzünde balık filtresinin altında dinamik olarak oluşturulan balık yönetim tablosunun yapısını ve teknik detaylarını açıklamaktadır.

---

## 🗂️ 1. İkon ve Klasör Yapısı

Balık ikonları varsayılan olarak aşağıdaki dizinde kategorize edilmiştir:
- **`Assets/FishIcons/rare/`**: Nadir balıklara ait `.png` ikonları.
- **`Assets/FishIcons/common/`**: Yaygın balıklara ait `.png` ikonları.

### Balık İsimlerinin Formatlanması
Dosya isimlerindeki alt tire (`_`) karakterleri temizlenerek her kelimenin ilk harfi büyük, kalan harfleri küçük olacak şekilde düzeltilmiştir (Örn: `Büyük_Sudak_Balığı.png` ➔ `Büyük Sudak Balığı`).

---

## 📊 2. Tablo Tasarımı ve Sütun Yapısı

Her kategori (`rare` ve `common`), `Sunny.UI.UIPanel` içinde ayrı bir kart/tablo olarak listelenir.

### Sütunlar ve İşlevleri:
1. **İkon & Balık Adı** (Sol): Balığın ikonu ve biçimlendirilmiş ismi.
2. **Balığı Tut** (Yeşil Checkbox): Balık yakalandığında envanterde tutulup tutulmayacağını belirler. (Varsayılan: `Checked = true`)
3. **Pişir** (Turuncu/Sarı Checkbox): Balığın pişirilip pişirilmeyeceğini belirler. (Varsayılan: `Checked = false`)
4. **Yere At** (Pembe/Kırmızı Checkbox): Balığın yere atılıp atılmayacağını belirler. (Varsayılan: `Checked = false`)

---

## 💻 3. İlgili Kod Dosyaları ve Değişiklikler

- **[FishBotPage.Designer.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Pages/FishBotPage.Designer.cs)**:
  - `fishFilterPanel` (`Sunny.UI.UIPanel`) bileşeni eklenerek balık filtresi altındaki dinamik alan konumlandırıldı.

- **[FishBotPage.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Pages/FishBotPage.cs)**:
  - `BuildFishFilterTable()` ve `CreateFishCategoryTable()` metodları eklenerek dosya sistemindeki resimler dinamik olarak okundu ve tabloya dönüştürüldü.
