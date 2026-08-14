using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using Aether.States;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Balık botu ilk başlatıldığında SADECE 1 KERE çalışan başlangıç hazırlık fonksiyonudur.
    /// Modüler yapıda bağımsız bir fonksiyon olarak tasarlanmıştır.
    /// 
    /// AKIŞ SIRASI:
    /// 1. Seçili olan oyun penceresini ekranda en öne getirir.
    /// 2. Pencerenin ortasına insansı kavisle fareyi taşıyıp 1 kere sağ tıklar (Right Click).
    /// 3. 'F' tuşuna kesintisiz 3 saniye (3000 ms) basılı tutup bırakır.
    /// 4. 'G' tuşuna kesintisiz 3 saniye (3000 ms) basılı tutup bırakır.
    /// 5. EquipmentMenuTitle aranır; bulunamazsa 'I' tuşuna basılıp 100ms aralıklarla aranır.
    /// 6. Menü bulunduğunda EquipmentMenuExitButton aranır ve fare insansı kavisle gidip tıklayarak menüyü kapatır.
    /// 7. InventoryPagesPosition taranarak hangi envanter sayfasının açık olduğu kontrol edilir.
    ///    Client ayarlarındaki sayfa numarası (InventoryPage) seçili değilse insansı kavisle o sayfaya tıklanır.
    /// </summary>
    public static class FishBotStartupFunction
    {
        /// <summary>
        /// Başlangıç sekansını verilen istemci için asenkron olarak çalıştırır.
        /// </summary>
        /// <param name="clientInfo">İşlem yapılacak aktif istemci bilgisi</param>
        /// <param name="cancellationToken">İptal isteği bayrağı</param>
        public static async Task ExecuteAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || cancellationToken.IsCancellationRequested) return;

            // HWND Geçerlilik Kontrolü
            if (clientInfo.Handle == IntPtr.Zero || !Win32Native.IsWindow(clientInfo.Handle))
            {
                Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) için geçerli pencere bulunamadı.");
                return;
            }

            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç sekansı başlatılıyor...");

            // 1. Bot çalışır çalışmaz seçili olan oyun penceresi en öne getirilecek
            GameWindowProcessHelper.BringWindowToFront(clientInfo.Handle);
            await Task.Delay(400, cancellationToken);

            // 2. F ve G tuşlarına basmadan önce pencerenin ortasına insansı kavisle gidip 1 kere sağ tıklayıp bıraksın
            await PerformWindowCenterRightClickAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 3. Sırayla önce F tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'F' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_F, 3000, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'F' tuşu bırakıldı.");

            await Task.Delay(300, cancellationToken);

            // 4. Sonra G tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'G' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_G, 3000, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> 'G' tuşu bırakıldı.");

            await Task.Delay(300, cancellationToken);

            // 5 & 6. Ekipman menüsü kontrolü, 'I' tuşu döngüsü ve Exit Button insansı tıklaması
            await EnsureEquipmentMenuClosedAsync(clientInfo, cancellationToken);

            await Task.Delay(300, cancellationToken);

            // 7. Envanter sayfası kontrolü ve Client ayarındaki sayfaya insansı tıklama
            await EnsureInventoryPageSelectedAsync(clientInfo, cancellationToken);

            await Task.Delay(300, cancellationToken);
            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç sekansı başarıyla tamamlandı.");
        }

        /// <summary>
        /// EquipmentMenuTitlePosition alanında EquipmentMenuTitle şablonunu arar.
        /// Bulunamazsa 'I' tuşuna basarak (100ms aralıkla) menü açılana kadar tarar.
        /// Menü açıldığında EquipmentMenuExitButtonPosition alanındaki kapatma butonunu tespit eder,
        /// Windows faresini insansı kavisle (Bézier) butonun üzerine götürüp tıklar ve menüyü kapatır.
        /// </summary>
        public static async Task EnsureEquipmentMenuClosedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> Ekipman menüsü kontrol ediliyor...");

            bool titleFound = false;
            int maxAttempts = 15;

            // 1. EquipmentMenuTitle aranıyor, yoksa I tuşuna basılıp 100ms bekleniyor
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                using (Bitmap? titleBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.EquipmentMenuTitlePosition))
                {
                    if (titleBmp != null)
                    {
                        var match = TemplateConstants.Match(titleBmp, TemplateConstants.WindowParts.EquipmentMenuTitle, threshold: 0.80);
                        if (match.IsSuccess)
                        {
                            Debug.WriteLine($"[FishBotStartupFunction] EquipmentMenuTitle bulundu! (Deneme #{attempt}, Güven: %{match.Confidence * 100:F1})");
                            titleFound = true;
                            break;
                        }
                    }
                }

                // Bulunamadıysa I tuşuna bas ve 100ms bekle
                Debug.WriteLine($"[FishBotStartupFunction] EquipmentMenuTitle bulunamadı, 'I' tuşuna basılıyor (Deneme #{attempt})...");
                byte iScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_I, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_I, iScan, 0, 0);
                await Task.Delay(40, cancellationToken);
                Win32Native.keybd_event((byte)Win32Native.VK_I, iScan, Win32Native.KEYEVENTF_KEYUP, 0);

                await Task.Delay(100, cancellationToken);
            }

            if (!titleFound)
            {
                Debug.WriteLine($"[FishBotStartupFunction] UYARI: {maxAttempts} denemede EquipmentMenuTitle bulunamadı.");
                return;
            }

            await Task.Delay(150, cancellationToken);

            // 2. EquipmentMenuExitButtonPosition alanında ExitButton şablonu aranıyor ve insansı kavisle tıklanıyor
            using (Bitmap? exitBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.EquipmentMenuExitButtonPosition))
            {
                int clickLocalX = RegionConstants.EquipmentMenuExitButtonPosition.StartX + (RegionConstants.EquipmentMenuExitButtonPosition.Width / 2);
                int clickLocalY = RegionConstants.EquipmentMenuExitButtonPosition.StartY + (RegionConstants.EquipmentMenuExitButtonPosition.Height / 2);

                if (exitBmp != null)
                {
                    var exitMatch = TemplateConstants.Match(exitBmp, TemplateConstants.WindowParts.EquipmentMenuExitButton, threshold: 0.75);
                    if (exitMatch.IsSuccess)
                    {
                        clickLocalX = RegionConstants.EquipmentMenuExitButtonPosition.StartX + exitMatch.Location.X + (exitMatch.Bounds.Width / 2);
                        clickLocalY = RegionConstants.EquipmentMenuExitButtonPosition.StartY + exitMatch.Location.Y + (exitMatch.Bounds.Height / 2);
                        Debug.WriteLine($"[FishBotStartupFunction] EquipmentMenuExitButton tespit edildi! Konum: ({clickLocalX}, {clickLocalY}) Güven: %{exitMatch.Confidence * 100:F1}");
                    }
                }

                // Windows faresini insansı kavisle götür ve sol tıkla
                Debug.WriteLine($"[FishBotStartupFunction] İnsansı fare hareketiyle Exit Button'a tıklanıyor: ({clickLocalX}, {clickLocalY})");
                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, clickLocalX, clickLocalY, fastMove: false, cancellationToken);
            }

            await Task.Delay(250, cancellationToken);
        }

        /// <summary>
        /// InventoryPagesPosition alanında 1, 2, 3, 4 sayfalarının açık/kapalı durumunu tespit eder.
        /// Client ayarlarında tanımlı hedef sayfa (InventoryPage) seçili değilse,
        /// Windows faresini insansı kavisle ilgili sayfa butonuna götürüp tıklar.
        /// </summary>
        public static async Task EnsureInventoryPageSelectedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            // İstemcinin kayıtlı balık botu ayarlarından hedef sayfa numarasını al (1, 2, 3 veya 4)
            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);
            int targetPage = Math.Clamp(settings?.InventoryPage ?? 1, 1, 4);

            Debug.WriteLine($"[FishBotStartupFunction] Client #{clientInfo.Id} -> Hedef Envanter Sayfası: {targetPage}. Sayfa durumu taranıyor...");

            using (Bitmap? pagesBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPagesPosition))
            {
                if (pagesBmp == null)
                {
                    Debug.WriteLine("[FishBotStartupFunction] InventoryPagesPosition ekran görüntüsü alınamadı.");
                    return;
                }

                // Açık/Aktif olan sayfa şablonları
                string[] activeTemplates = {
                    TemplateConstants.WindowParts.Page1Acik,
                    TemplateConstants.WindowParts.Page2Acik,
                    TemplateConstants.WindowParts.Page3Acik,
                    TemplateConstants.WindowParts.Page4Acik
                };

                // Kapalı/Pasif olan sayfa şablonları
                string[] closedTemplates = {
                    TemplateConstants.WindowParts.Page1,
                    TemplateConstants.WindowParts.Page2,
                    TemplateConstants.WindowParts.Page3,
                    TemplateConstants.WindowParts.Page4
                };

                int currentlyActivePage = -1;
                double maxActiveConfidence = 0;

                // 1. Hangi sayfanın şu an açık olduğunu tespit et
                for (int p = 0; p < 4; p++)
                {
                    var activeMatch = TemplateConstants.Match(pagesBmp, activeTemplates[p], threshold: 0.70);
                    if (activeMatch.IsSuccess && activeMatch.Confidence > maxActiveConfidence)
                    {
                        maxActiveConfidence = activeMatch.Confidence;
                        currentlyActivePage = p + 1;
                    }
                }

                Debug.WriteLine($"[FishBotStartupFunction] Tespit edilen aktif sayfa: {(currentlyActivePage > 0 ? currentlyActivePage.ToString() : "Bilinmiyor")} (Güven: %{maxActiveConfidence * 100:F1})");

                // Eğer hedeflenen sayfa zaten açıksa hiçbir işlem yapma
                if (currentlyActivePage == targetPage)
                {
                    Debug.WriteLine($"[FishBotStartupFunction] Sayfa {targetPage} zaten aktif durumda, tıklamaya gerek yok.");
                    return;
                }

                // 2. Hedef sayfa kapalıysa butonun konumunu tespit et
                string targetClosedTemplate = closedTemplates[targetPage - 1];
                var buttonMatch = TemplateConstants.Match(pagesBmp, targetClosedTemplate, threshold: 0.70);

                int clickLocalX;
                // Sayfa butonlarının dikey eksende kesinlikle tam ortasına basması için bölge merkezini sabitle
                int clickLocalY = RegionConstants.InventoryPagesPosition.StartY + (RegionConstants.InventoryPagesPosition.Height / 2);

                if (buttonMatch.IsSuccess)
                {
                    clickLocalX = RegionConstants.InventoryPagesPosition.StartX + buttonMatch.Location.X + (buttonMatch.Bounds.Width / 2);
                    Debug.WriteLine($"[FishBotStartupFunction] Sayfa {targetPage} butonu OpenCV ile bulundu: ({clickLocalX}, {clickLocalY}) Güven: %{buttonMatch.Confidence * 100:F1}");
                }
                else
                {
                    // Fallback: 4 buton için yatay orantısal merkez hesaplama (1..4)
                    int regionWidth = RegionConstants.InventoryPagesPosition.Width;
                    int segmentWidth = regionWidth / 4;
                    clickLocalX = RegionConstants.InventoryPagesPosition.StartX + (segmentWidth * (targetPage - 1)) + (segmentWidth / 2);
                    Debug.WriteLine($"[FishBotStartupFunction] Sayfa {targetPage} şablonu tam eşleşmedi, orantısal koordinata tıklanacak: ({clickLocalX}, {clickLocalY})");
                }

                // 3. Windows faresini insansı kavisle tam ortaya götür ve garanti olması için 3-6 defa rastgele tıkla
                int clickCount = Random.Shared.Next(3, 7); // 3, 4, 5 veya 6 defa
                Debug.WriteLine($"[FishBotStartupFunction] İnsansı fare hareketiyle Sayfa {targetPage} butonunun tam merkezine ({clickLocalX}, {clickLocalY}) {clickCount} kez tıklanıyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, clickLocalX, clickLocalY, fastMove: false, clickCount: clickCount, cancellationToken: cancellationToken);
            }

            await Task.Delay(250, cancellationToken);
        }

        /// <summary>
        /// Belirtilen sanal tuşa (VK) verilen süre boyunca basılı tutar.
        /// Oyun motorlarının (DirectX / DirectInput) tuş basılı tutma durumunu kesintisiz algılaması için
        /// periyodik donanımsal tekrar (typematic repeat) darbeleri üretir ve süre bitiminde tuşu serbest bırakır.
        /// </summary>
        public static async Task HoldKeyAsync(uint vk, int durationMs, CancellationToken cancellationToken)
        {
            byte scanCode = (byte)Win32Native.MapVirtualKey(vk, 0);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // İlk tuş basma (Key Down)
                Win32Native.keybd_event((byte)vk, scanCode, 0, 0);

                while (stopwatch.ElapsedMilliseconds < durationMs)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    // DirectInput / Oyun döngülerinin tuşun basılı kaldığını sürekli hissetmesi için repeat sinyali
                    Win32Native.keybd_event((byte)vk, scanCode, 0, 0);
                    await Task.Delay(40, cancellationToken);
                }
            }
            finally
            {
                // Tuşu garanti olarak serbest bırak (Key Up)
                Win32Native.keybd_event((byte)vk, scanCode, Win32Native.KEYEVENTF_KEYUP, 0);
            }
        }

        /// <summary>
        /// Verilen pencerenin merkez koordinatını hesaplayıp fareyi insansı kavisle oraya taşır ve sağ tıklar.
        /// </summary>
        public static async Task PerformWindowCenterRightClickAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd)) return;

                int localX = 400;
                int localY = 300;

                if (Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect) && clientRect.Width > 0 && clientRect.Height > 0)
                {
                    localX = clientRect.Width / 2;
                    localY = clientRect.Height / 2;
                }

                Debug.WriteLine($"[FishBotStartupFunction] Pencere merkezine insansı fare ile sağ tıklanıyor: ({localX}, {localY})");
                await HumanMouseService.Instance.RightClickLocalAsync(hWnd, localX, localY, fastMove: false, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FishBotStartupFunction] Sağ tık hatası: {ex.Message}");
            }
        }
    }
}
