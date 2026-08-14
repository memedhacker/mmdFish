# 🖱️ İnsansı Fare (Human-like Mouse) Otomasyon Rehberi

Bu kılavuz, **Aether** projesinde Windows'un gerçek fiziksel fare imlecini insan elini taklit eden rastgele kavisli (Cubic Bézier) hareketler, yumuşak hızlanma-yavaşlama (Ease-in-out) ve donanımsal tıklamalar ile kontrol eden [`Services/HumanMouseService.cs`](file:///c:/Users/mehme/Documents/GitHub/mmdFish/Aether/Services/HumanMouseService.cs) mimarisini açıklar.

---

## 🌟 1. Mimari Genel Bakış (`HumanMouseService`)

`HumanMouseService`, Windows `SetCursorPos` ve `mouse_event` API'lerini kullanarak fiziksel fareyi ekranda gözle görülür şekilde pürüzsüz ve insansı bir kavisle hedefe taşır.

```text
[Hedef Koordinat] ──> HumanMouseService.Instance.MoveMouseAsync(x, y)
                            │
                            ├──> Kübik Bézier Kontrol Noktaları ($P_0 \to P_1 \to P_2 \to P_3$)
                            ├──> EaseInOutCubic Hızlanma & Yavaşlama S-Eğrisi
                            ├──> Mikro Kas Titremesi (Micro Jitter $\pm 1$ px)
                            └──> Adım Adım Windows SetCursorPos (60-120 FPS)
```

---

## 🧠 2. İnsansı Hareket Algoritması Nasıl Çalışır?

Anti-cheat sistemleri fareyi aniden ışınlayan (teleport) veya cetvelle çizilmiş gibi dümdüz hareket ettiren robotik davranışları kolayca tespit eder. `HumanMouseService` 4 temel prensip uygular:

### 1. Kübik Bézier Eğrisi (Cubic Bézier Curve)
Başlangıç noktası ($P_0$) ile varış noktası ($P_3$) arasında insan bileğinin doğal salınımını taklit eden 2 rastgele kontrol noktası ($P_1, P_2$) üretilir:

$$B(t) = (1-t)^3 P_0 + 3(1-t)^2 t P_1 + 3(1-t) t^2 P_2 + t^3 P_3 \quad (0 \le t \le 1)$$

### 2. S-Eğrisi Hızlanma ve Yavaşlama (Ease-in-out Cubic)
İmleç kalkış anında yavaşça hızlanır, yolun ortasında maksimum süratine ulaşır ve hedef noktaya varırken fren yaparak yumuşakça durur:

```csharp
private static double EaseInOutCubic(double t)
{
    return t < 0.5
        ? 4 * t * t * t
        : 1 - Math.Pow(-2 * t + 2, 3) / 2;
}
```

### 3. Mikro Kas Titremesi (Micro Jitter)
Yol boyunca her adımda fareye $\pm 0.6$ piksel aralığında doğal insan eli kas titreşimi eklenir.

---

## ⚡ 3. Normal Hareket vs. Hızlı Hareket (Fast Move)

| Özellik | Normal İnsansı Hareket (`MoveMouseAsync`) | Hızlı İnsansı Hareket (`MoveMouseFastAsync`) |
| :--- | :--- | :--- |
| **Kullanım Alanı** | Menü kapatma, buton tıklama, rutin işlemler | Acil mini-game tepkileri, balık kaçırma önleme |
| **Süre** | $180\text{ ms} - 500\text{ ms}$ (Mesafeye göre dinamik) | $60\text{ ms} - 180\text{ ms}$ (Çok çevik ve atik) |
| **Kavis Açısı** | Geniş doğal kavis (0.26 katsayı) | Dar ve odaklanmış kavis (0.15 katsayı) |
| **Adım Sayısı** | 16 - 45 adım | 6 - 18 adım |

---

## 💻 4. Kodlama ve Kullanım Örnekleri

Servis Singleton olarak `HumanMouseService.Instance` üzerinden doğrudan çağrılır. Hem masaüstü ekran koordinatı (Screen X, Y) hem de oyun penceresi yerel koordinatı (Local X, Y) ile çalışabilir.

### Örnek 1: Normal İnsansı Hareket ve Sol Tıklama
```csharp
using Aether.Services;

// Oyun penceresi içi (Local: 780, 20) koordinatına insansı kavisle git ve sol tıkla:
await HumanMouseService.Instance.LeftClickLocalAsync(
    hWnd: clientInfo.Handle,
    localX: 780,
    localY: 20,
    fastMove: false,
    cancellationToken: cancellationToken
);
```

---

### Örnek 2: Hızlı İnsansı Hareket (Fast Move - İleride Kullanım İçin)
```csharp
// Çok hızlı ama kavisini koruyan hareket ile hedefe git:
await HumanMouseService.Instance.MoveMouseFastToLocalAsync(
    hWnd: clientInfo.Handle,
    localX: 400,
    localY: 300,
    cancellationToken: cancellationToken
);
```

---

### Örnek 3: Doğal Sağ Tıklama
```csharp
// Pencerenin merkezine insansı kavisle gidip sağ tıkla:
await HumanMouseService.Instance.RightClickLocalAsync(
    hWnd: clientInfo.Handle,
    localX: 400,
    localY: 300,
    fastMove: false,
    cancellationToken: cancellationToken
);
```

---

## 🛡️ 5. Güvenlik ve Anti-Ban Önlemleri

1. **Donanımsal Olaylar:** Doğrudan Windows `mouse_event` seviyesinde simüle edilir; hiçbir DLL enjeksiyonu veya bellek yaması gerektirmez.
2. **Doğal Tıklama Gecikmeleri:** 
   - Tıklama öncesi insan duraksaması: $35-70\text{ ms}$
   - Buton basılı tutma süresi: $40-80\text{ ms}$
3. **Piksel Hedef Sapması:** Butonların hep aynı merkez pikseline değil, $\pm 1-2$ piksellik doğal varyasyonla tıklar.
