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
    /// Balık botu ilk başlatıldığında SADECE 1 KERE çalışan merkezi orkestrasyon (başlangıç hazırlık) sınıfıdır.
    /// Modüler alt fonksiyonları sırayla çalıştırır.
    /// 
    /// AKIŞ SIRASI:
    /// 1. Seçili olan oyun penceresini ekranda en öne getirir.
    /// 2. Pencerenin ortasına insansı kavisle fareyi taşıyıp 1 kere sağ tıklar (Right Click).
    /// 3. 'F' tuşuna kesintisiz 3 saniye basılı tutup bırakır.
    /// 4. 'G' tuşuna kesintisiz 3 saniye basılı tutup bırakır.
    /// 5. EquipmentMenuTitle aranır; bulunamazsa 'I' tuşuna basılıp 100ms aralıklarla aranır.
    /// 6. Menü bulunduğunda EquipmentMenuExitButton aranır ve fare insansı kavisle gidip tıklayarak menüyü kapatır.
    /// 7. InventoryPagesPosition taranarak hangi envanter sayfasının açık olduğu kontrol edilir.
    ///    Client ayarlarındaki sayfa numarası (InventoryPage) seçili değilse insansı kavisle o sayfaya tıklanır.
    /// 8. Envanterdeki yemler (%99.0 eşik) taranıp birbiri üstüne sürüklenerek birleştirilir (Stacklenir).
    /// 9. Kamp ateşlerini (ates.png) bul ve ilk 3 slota yerleştir.
    /// 10. Yemler/solucanlar envanterin en altındaki boş slotlara taşınır.
    /// 11. BuyWorm aktifse balıkçıdan yem satın alınır.
    /// 12. InventoryFishArea boşluk kontrolü yapılır; boş slot yoksa pişirme süreci işletilir, açılamazsa bot durdurulur.
    /// </summary>
    public static class FishBotStartupFunction
    {
        /// <summary>
        /// Başlangıç sekansını verilen istemci için asenkron olarak sırayla çalıştırır.
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

            BotLogger.LogInfo(clientInfo.Id, $"Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç hazırlık sekansı başlatıldı.");

            // 1. Bot çalışır çalışmaz seçili olan oyun penceresi en öne getirilecek
            GameWindowProcessHelper.BringWindowToFront(clientInfo.Handle);
            await Task.Delay(400, cancellationToken);

            // 2. Pencerenin ortasına insansı kavisle gidip 1 kere sağ tıklayıp bıraksın
            BotLogger.LogInfo(clientInfo.Id, "Ekran ortasına odaklanma sağ tıkı yapılıyor...");
            await StartupCameraFunction.PerformWindowCenterRightClickAsync(clientInfo.Handle, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 3 & 4. F ve G tuşlarına 3'er saniye basılı tutulup kamera ayarlanacak
            BotLogger.LogInfo(clientInfo.Id, "Kamera açısı ayarlanıyor (G ve F tuşları)...");
            await StartupCameraFunction.ExecuteCameraPreparationAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 5 & 6. Ekipman menüsü kontrolü, 'I' tuşu döngüsü ve Exit Button insansı tıklaması
            BotLogger.LogInfo(clientInfo.Id, "Ekipman menüsü kontrol ediliyor...");
            await StartupEquipmentMenuFunction.EnsureEquipmentMenuClosedAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 7. Envanter sayfası kontrolü ve Client ayarındaki sayfaya insansı tıklama
            BotLogger.LogInfo(clientInfo.Id, "Envanter sayfası kontrol ediliyor...");
            await StartupInventoryPageFunction.EnsureInventoryPageSelectedAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 8. Envanterdeki yemleri kontrol et ve gerekirse birleştir (Stackle)
            BotLogger.LogInfo(clientInfo.Id, "Yemler taranıyor ve birleştiriliyor (Stack)...");
            await StartupBaitOrganizerFunction.StackInventoryBaitsAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 9. Kamp ateşlerini (ates.png) bul ve ilk 3 slota yerleştir (3'ten fazlaysa üst üste)
            BotLogger.LogInfo(clientInfo.Id, "Kamp ateşleri ilk 3 slota düzenleniyor...");
            await StartupBaitOrganizerFunction.OrganizeCampfiresToFirstThreeSlotsAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 10. Yemleri / solucanları envanterin 4. slot ve sonrasındaki boşluklara taşı
            BotLogger.LogInfo(clientInfo.Id, "Yemler 4. slot ve sonrasına taşınıyor...");
            await StartupBaitOrganizerFunction.MoveBaitsToBottomEmptySlotsAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 11. Eğer BuyWorm aktif ve 4. slot sonrasında boş yer varsa: Balıkçıyı bul ve marketi aç
            await StartupFishermanFunction.ExecuteAsync(clientInfo, cancellationToken);
            await Task.Delay(300, cancellationToken);

            // 12. Envanter balık slotları (InventoryFishArea) boşluk kontrolü
            BotLogger.LogInfo(clientInfo.Id, "[Başlangıç] InventoryFishArea boş slot sayısı kontrol ediliyor...");
            await StartupBaitOrganizerFunction.MoveMouseOutsideInventoryAsync(clientInfo.Handle, cancellationToken);

            int emptyCount = FishingExecutionFunction.ScanEmptySlots(clientInfo.Handle);
            BotLogger.LogInfo(clientInfo.Id, $"[Başlangıç] InventoryFishArea boş slot sayısı: {emptyCount}");

            if (emptyCount == 0)
            {
                BotLogger.LogWarning(clientInfo.Id, "🛑 [Başlangıç] InventoryFishArea içerisinde hiç boş slot kalmadı (EmptySlot: 0)! Önce balık öldürme başlatılıyor...");

                var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);
                await FishKillingFunction.ExecuteKillingProcessAsync(clientInfo, settings, cancellationToken);

                BotLogger.LogInfo(clientInfo.Id, "🗑️ [Başlangıç] Öldürme tamamlandı, yere atma sürecine geçiliyor...");
                await FishDropFunction.ExecuteDropProcessAsync(clientInfo, settings, cancellationToken);

                BotLogger.LogInfo(clientInfo.Id, "🔥 [Başlangıç] Yere atma tamamlandı, balık pişirme sürecine geçiliyor...");
                bool cookedSuccess = await FishCookingFunction.ExecuteCookingProcessAsync(clientInfo, settings, cancellationToken);

                if (!cookedSuccess)
                {
                    return;
                }
            }

            BotLogger.LogSuccess(clientInfo.Id, $"Client #{clientInfo.Id} ({clientInfo.Name}) başlangıç sekansı başarıyla tamamlandı.");
        }

        #region Geriye Uyumluluk (Backward Compatibility Wrappers)

        public static Task EnsureEquipmentMenuClosedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupEquipmentMenuFunction.EnsureEquipmentMenuClosedAsync(clientInfo, cancellationToken);

        public static Task EnsureInventoryPageSelectedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupInventoryPageFunction.EnsureInventoryPageSelectedAsync(clientInfo, cancellationToken);

        public static Task StackInventoryBaitsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupBaitOrganizerFunction.StackInventoryBaitsAsync(clientInfo, cancellationToken);

        public static Task OrganizeCampfiresToFirstThreeSlotsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupBaitOrganizerFunction.OrganizeCampfiresToFirstThreeSlotsAsync(clientInfo, cancellationToken);

        public static Task MoveBaitsToBottomEmptySlotsAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupBaitOrganizerFunction.MoveBaitsToBottomEmptySlotsAsync(clientInfo, cancellationToken);

        public static Task ExecuteFishermanSequenceAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
            => StartupFishermanFunction.ExecuteAsync(clientInfo, cancellationToken);

        public static Task HoldKeyAsync(uint vk, int durationMs, CancellationToken cancellationToken)
            => StartupCameraFunction.HoldKeyAsync(vk, durationMs, cancellationToken);

        public static Task PerformWindowCenterRightClickAsync(IntPtr hWnd, CancellationToken cancellationToken)
            => StartupCameraFunction.PerformWindowCenterRightClickAsync(hWnd, cancellationToken);

        #endregion
    }
}
