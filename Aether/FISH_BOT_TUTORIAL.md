# 📘 Balık Botu Geliştirici, Çalıştırma ve İptal (Tutorial) Rehberi

Bu doküman, **Aether** balık botunun mimarisini, dairesel döngünün nerede çalıştığını, acil durumda nasıl kapatılacağını, farklı bir buton üzerinden nasıl başlatılacağını / durdurulacağını ve yeni adımların kod içerisine nasıl ekleneceğini adım adım açıklamaktadır.

---

## 🏛️ 1. Mimari Yapı ve Bileşenler

Balık botu sistemi, Katmanlı Mimari (Layered Architecture) ve İptal Mekanizmalı Asenkron Görevler (`Task.Run` + `CancellationTokenSource`) prensibine dayanır:

```
┌─────────────────────────────────────────────────────────┐
│ Controls/ClientCard.cs (startClient Butonu)             │
└───────────────────────────┬─────────────────────────────┘
                            │ (OnStartClientClicked Olayı)
                            ▼
┌─────────────────────────────────────────────────────────┐
│ Services/FishBotService.cs (Singleton Döngü Yöneticisi) │
└───────────────────────────┬─────────────────────────────┘
                            │ (Arka Plan Task & CancellationToken)
                            ▼
┌─────────────────────────────────────────────────────────┐
│ Helpers/FishBotEngineHelper.cs (Modüler Mantık Katmanı) │
└───────────────────────────┬─────────────────────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        ▼                                       ▼
Services/InputAutomationService.cs    Helpers/WindowCaptureHelper.cs
(Tuş & Fare Simülasyonu)              (Arka Plan HWND Resim Alma)
```

---

## 🔄 2. Sıkça Sorulan Sorular ve Kullanım Rehberi

### 1. 🔄 Döngü Nerede Dönüyor?
Botun kesintisiz dairesel döngüsü **[Services/FishBotService.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Services/FishBotService.cs)** içerisindeki `FishBotLoopAsync` metodunda dönmektedir:

```csharp
private async Task FishBotLoopAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
{
    try
    {
        // 🔄 DÖNGÜ BURADA DÖNMEKTEDİR:
        while (!cancellationToken.IsCancellationRequested)
        {
            // Her dairesel turda modüler helper metodunu çağırır
            await Helpers.FishBotEngineHelper.ExecuteSingleCycleAsync(clientInfo, cancellationToken);
        }
    }
    catch (OperationCanceledException)
    {
        // Bot durdurulduğunda fırlatılan normal iptal istisnası
    }
}
```

---

### 2. 🚨 Acil Durumda Nasıl Kapatılır?
Acil durumlarda veya uygulamanın herhangi bir yerinden botları anında kapatmak için iki farklı yöntem mevcuttur:

1. **Tekil İstemciyi Acil Durdurma**:
   ```csharp
   Services.FishBotService.Instance.StopFishBot(clientId);
   ```
2. **Çalışan Tüm İstemcileri Anında Kapatma (Acil Stop / Kill All)**:
   ```csharp
   Services.FishBotService.Instance.StopAllBots();
   ```

> **Not:** `CancellationToken.ThrowIfCancellationRequested()` çağrısı sayesinde arka plan görevi UI thread'ini dondurmadan milisaniyeler içinde anında iptal edilir.

---

### 3. 🔘 Başka Bir Butona Basınca Nasıl Tetiklenir (Başlatılır)?
Uygulamadaki herhangi bir Form, UserControl veya butonun Click event handler'ı içerisinden botu başlatabilirsiniz:

```csharp
private void btnStartCustom_Click(object sender, EventArgs e)
{
    // Aktif seçili client'ı veya Id ile hedef client'ı al
    var clientInfo = ClientState.Instance.SelectedClient; 

    if (clientInfo == null)
    {
        MessageBox.Show("Lütfen önce bir client seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // Botu başlat (HWND seçilmemişse uyarı mesajı döner)
    var (success, message) = Services.FishBotService.Instance.StartFishBot(clientInfo);

    if (!success)
    {
        MessageBox.Show(message, "HWND Eksik Uyarısı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
```

Veya durumu tersine çevirmek (çalışıyorsa durdur, duruyorsa başlat) için:
```csharp
Services.FishBotService.Instance.ToggleFishBot(clientInfo);
```

---

### 4. 🛑 Farklı Bir Butonun Click Eventi İçerisinden veya Başka Bir Yerden Nasıl Kapatılır?
Farklı bir butonun (Örn: "Seçilenleri Durdur" veya "Tümünü Durdur" butonu) Click olayından botu güvenle kapatabilirsiniz:

```csharp
// Örnek: "Seçilen Client'ı Durdur" Buton Tıklaması
private void btnStopSelected_Click(object sender, EventArgs e)
{
    if (ClientState.Instance.SelectedClient != null)
    {
        int selectedId = ClientState.Instance.SelectedClient.Id;
        Services.FishBotService.Instance.StopFishBot(selectedId);
    }
}

// Örnek: "Tüm Botları Durdur" Buton Tıklaması
private void btnStopAll_Click(object sender, EventArgs e)
{
    Services.FishBotService.Instance.StopAllBots();
}
```

---

## 📍 3. Yeni Bot Adımları Nereye ve Nasıl Eklenir?

Tüm balık tutma iş mantığı ve adım dizilimleri **[Helpers/FishBotEngineHelper.cs](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Helpers/FishBotEngineHelper.cs)** dosyası içerisinde toplanmıştır.

`ExecuteSingleCycleAsync` metodu her balık tutma döngüsünde sırayla çalışan ana orkestrasyondur:

```csharp
public static async Task ExecuteSingleCycleAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
{
    if (clientInfo == null || cancellationToken.IsCancellationRequested) return;

    // 1. Pencere Doğrulama
    if (!ValidateGameWindow(clientInfo)) return;

    // 📍 [BURAYA YENİ ADIMLAR EKLENİR]
    // Örnek: Ekran resmi alma, OpenCV tespitleri, tuş basımları vb.

    await Task.Delay(500, cancellationToken);
}
```

---

## ⚡ 4. Asenkron & İptal (CancellationToken) Kuralları

- **Thread-Safety**: Bot döngüsü UI thread'ini asla bloklamaz (`Task.Run` kullanılır).
- **Duyarlı İptal**: Yazdığınız her adım fonksiyonunun başına `cancellationToken.ThrowIfCancellationRequested();` ekleyin.
- **Beklemeler**: `Thread.Sleep` yerine her zaman `await Task.Delay(ms, cancellationToken)` tercih edilmelidir.
