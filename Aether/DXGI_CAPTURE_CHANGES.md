# 🛡️ DXGI Desktop Duplication ve Bölgesel Ekran Yakalama Değişiklikleri

Bu belge, oyun koruma yazılımlarının (Anti-Cheat) görsel yakalama engellerini (siyah ekran) aşmak ve güvenli ekran okuma sağlamak amacıyla yapılan mimari değişiklikleri özetlemektedir.

---

## 1. 🎯 Amaç ve Güvenli Geliştirme Mantığı

* **Yalnızca Görsel Okuma (Read-Only):** Kod enjeksiyonu veya bellek yazma (`WriteProcessMemory`, DLL Injection vb.) yapılmadığı için en düşük riskli yöntemdir.
* **Siyah Ekran Sorununun Çözümü:** Anti-cheat yazılımları, doğrudan oyun penceresine (`HWND`) atılan `PrintWindow` veya `GetDC` gibi Win32 API çağrılarını engelleyerek siyah ekran döndürür.
* **DXGI (Desktop Duplication) Yaklaşımı:** Oyuna veya pencere tutacağına doğrudan istek atmak yerine, Windows DWM / GPU sürücüsü katmanında (`Direct3D 11` & `IDXGIOutputDuplication`) masaüstünün GPU üzerindeki karesi okunur.
* **Koordinat Eşleme ve Kırpma (Screen Coordinate Mapping & Crop):** GPU'dan alınan tam masaüstü karesi, oyun penceresinin masaüstündeki koordinatlarına (`ClientToScreen` / `GetWindowRect`) göre milisaniyeler içinde kırpılır.

---

## 2. 📂 Eklenen ve Güncellenen Dosyalar

### 1. `Native/DxgiDesktopDuplicator.cs` (YENİ)
* **Açıklama:** Windows DirectX Graphics Infrastructure (DXGI) Desktop Duplication API sarmalayıcısı.
* **Özellikler:**
  * Direct3D 11 donanım cihazı (`D3D_DRIVER_TYPE_HARDWARE`) ve `IDXGIOutputDuplication` arayüzü yönetimi.
  * GPU tamponundaki ekran karesini CPU tarafından okunabilir Staging Texture'a kopyalama (`CopyResource`).
  * 32bpp ARGB formatında doğrudan bellek kopyalama (`Buffer.MemoryCopy`) ile yüksek hızlı kare yakalama.
  * Belirli bir ekran dikdörtgenini doğrudan masaüstünden kırpan `CaptureScreenRegion` fonksiyonu.
  * Zaman aşımı, donanım sıfırlanması veya çözünürlük değişimlerine karşı otomatik yeniden ilklendirme.

---

### 2. `Helpers/WindowRegionCaptureHelper.cs` (GÜNCELLENDİ)
* **Eklenen `WindowCaptureMode` Enum Değerleri:**
  * `Auto` (Varsayılan): Önce DXGI Desktop Duplication dener, başarısız olursa sürücü seviyesi GDI masaüstü kırpma (`DesktopCropGdi`), son olarak standart `PrintWindow` dener.
  * `DxgiDesktopDuplication`: Doğrudan GPU seviyesinde masaüstü yakalama ve pencere koordinat kırpma.
  * `DesktopCropGdi`: Ekran DC'si üzerinden koordinat kırpma.
  * `PrintWindow`: Klasik arka plan pencere yakalama.

* **Yeni ve Güncellenen Fonksiyonlar:**
  * `CaptureRegion(...)`: İstenen başlangıç ve bitiş koordinatlarını (`startX`, `startY`, `endX`, `endY`) DXGI ve akıllı hibrit mod desteğiyle yakalar.
  * `CaptureRegionViaDxgi(...)`: DXGI Desktop Duplication kullanarak pencerenin ilgili alanını keser.
  * `CaptureRegionViaDesktopCropGdi(...)`: Sürücü seviyesi GDI masaüstü DC'si üzerinden kırpma yapar.
  * `CalculateScreenTargetRect(...)`: İstemci içi yerel koordinatları masaüstü ekran koordinatlarına dönüştürür.
  * `PreviewFullWindowWithSelection(...)`: DXGI hibrit modu ile tam pencere koordinat seçim test ekranını açar.

---

## 3. 🚀 Kullanım Örnekleri

### A. Belirli Bir Alanı Yakalama (Template Matching İçin)
```csharp
using Aether.Helpers;
using System.Drawing;

// 1. Akıllı Hibrit (DXGI Öncelikli) Yakalama:
Bitmap? bolgeResmi = WindowRegionCaptureHelper.CaptureRegion(
    hWnd: client.Handle,
    startX: 100,
    startY: 150,
    endX: 300,
    endY: 350
);

// 2. Yalnızca DXGI Desktop Duplication ile Yakalama:
Bitmap? dxgiResmi = WindowRegionCaptureHelper.CaptureRegion(
    client.Handle,
    100, 150, 300, 350,
    restoreIfIconic: true,
    captureMode: WindowCaptureMode.DxgiDesktopDuplication
);
```

### B. Seçili İstemcinin Tam Ekran Koordinat Seçim Testini Açma
```csharp
var client = ClientState.Instance.SelectedClient;
if (client != null && client.Handle != IntPtr.Zero)
{
    var (success, message) = WindowRegionCaptureHelper.PreviewFullWindowWithSelection(
        client.Handle, 
        client.Name
    );
}
```
