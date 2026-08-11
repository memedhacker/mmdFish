# 🔤 Türkçe Karakter Kullanılan Kod Satırları

Bu dosya, Türkçe karakterlerin (ı, İ, ğ, Ğ, ş, Ş, ü, Ü, ö, Ö, ç, Ç) kullanıldığı kod satırlarını listeler.

> **Kural:** Kod ve identifier'lar İngilizce olmalıdır. Yorum ve dokümantasyon Türkçe olabilir.
> Aşağıdaki satırlar yorum dışında Türkçe karakter içeren **string literal** veya **identifier** içermektedir.

---

## 📂 Constants\Colors.cs — Identifier (Renk adları)

| Satır | İçerik |
|-------|--------|
| 12 | `public static readonly Color MaviKoyu = Color.FromArgb(0, 177, 255);` |
| 15 | `public static readonly Color MaviAcik = Color.FromArgb(89, 189, 255);` |
| 19 | `public static readonly Color PembeKoyu = Color.FromArgb(244, 103, 136);` |
| 22 | `public static readonly Color PembeAcik = Color.FromArgb(255, 139, 164);` |
| 26 | `public static readonly Color YesilKoyu = Color.FromArgb(99, 168, 71);` |
| 29 | `public static readonly Color YesilAcik = Color.FromArgb(135, 193, 109);` |
| 33 | `public static readonly Color Turuncu = Color.FromArgb(255, 180, 0);` |
| 37 | `public static readonly Color ArkaPlanKoyu = Color.FromArgb(24, 24, 27);` |
| 40 | `public static readonly Color ArkaPlanAcik = Color.FromArgb(30, 30, 35);` |
| 43 | `public static readonly Color CizgiRengi = Color.FromArgb(60, 60, 65);` |

---

## 📂 Controls\ClientCard.cs — String Literal

| Satır | İçerik |
|-------|--------|
| 92 | `if (!string.IsNullOrWhiteSpace(value) && value != "Client Seçilmedi")` |

---

## 📂 Controls\ClientsControl.cs — String Literal

| Satır | İçerik |
|-------|--------|
| 41 | `_currentlySelectedCard.GameWindowText = "Client Seçilmedi";` |

---

## 📂 Helpers\GameWindowProcessHelper.cs — String Literal

| Satır | İçerik |
|-------|--------|
| 74 | `comboBox.Items.Add("-- Seç --");` |

---

## 📂 Pages\BaseBotPage.cs — String Literal

| Satır | İçerik |
|-------|--------|
| 58 | `ClientNameLabel.Text = clientInfo?.Name ?? "Seçim Yok";` |

---

## 📂 Pages\FishBotPage.cs — String Literal (MessageBox mesajları)

| Satır | İçerik |
|-------|--------|
| 107 | `$"Bu pencere zaten '{existingOwner.Name}' (Client #{existingOwner.Id})'ye tanımlanmış durumda.",` |
| 108 | `"Pencere Seçim Hatası",` |
| 253 | `"Lütfen bir preset ismi girin.",` |
| 254 | `"Kayıt Hatası",` |
| 276 | `$"'{presetName}' başarıyla kaydedildi.",` |
| 277 | `"Preset Kaydedildi",` |
| 290 | `"Lütfen yüklenecek bir preset seçin.",` |
| 291 | `"Yükleme Hatası",` |
| 301 | `$"'{presetName}' preset dosyası okunamadı.",` |
| 302 | `"Yükleme Hatası",` |
| 328 | `"Lütfen silinecek bir preset seçin.",` |
| 329 | `"Silme Hatası",` |
| 336 | `$"'{presetName}' presetini kalıcı olarak silmek istediğinize emin misiniz?",` |
| 337 | `"Preset Sil",` |

---

## 📂 Services\InputAutomationService.cs — String Literal

| Satır | İçerik |
|-------|--------|
| 59 | `logger?.Invoke("Makro dizisi başarıyla tamamlandı.");` |
| 63 | `logger?.Invoke($"Makro Hatası: {ex.Message}");` |

---

## 📂 Helpers\FishFilterTableBuilder.cs — String Literal (yorum içine girmiş)

| Satır | İçerik |
|-------|--------|
| 181 | `// Binder'ın bu checkbox'ı tanımlayabilmesi için tag: "categoryId|itemKey|columnHeader"` *(satır içi yorum)* |

---

## 📌 Özet

| Kategori | Dosya | Öneri |
|----------|-------|-------|
| **Renk identifier'ları** | `Colors.cs` | İngilizce karşılıklar kullan (ör: `DarkGreen`, `LightBlue`) |
| **UI string literal'ları** | `ClientCard.cs`, `ClientsControl.cs`, `BaseBotPage.cs`, `GameWindowProcessHelper.cs` | Resource dosyasına taşı ya da İngilizce yaz |
| **MessageBox mesajları** | `FishBotPage.cs` | Kaynak dosyasına taşı ya da İngilizce yaz |
| **Logger mesajları** | `InputAutomationService.cs` | İngilizce yaz |

> **Not:** Designer.cs dosyaları ve `///` / `//` ile başlayan yorum satırları bu listeye dahil edilmemiştir.
