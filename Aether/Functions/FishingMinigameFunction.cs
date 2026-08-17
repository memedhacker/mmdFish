using Aether.Models;
using Aether.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Balık tutma mini oyununu yöneten modüler fonksiyon sınıfı.
    /// İleride balık tutma çemberi / mini oyun algoritmaları bu fonksiyona eklenecektir.
    /// Şimdilik arka planda periyodik olarak log üretir.
    /// </summary>
    public static class FishingMinigameFunction
    {
        /// <summary>
        /// Balık tutma mini oyun döngüsünü arka planda çalıştırır.
        /// </summary>
        /// <param name="clientInfo">İlgili istemci bilgisi</param>
        /// <param name="cancellationToken">İptal isteği bayrağı</param>
        public static async Task ExecuteMinigameAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null) return;

            BotLogger.LogInfo(clientInfo.Id, "Balık oyunu fonksiyonu başlatıldı.");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    BotLogger.LogInfo(clientInfo.Id, "Balık tutuluyor...");

                    // 📍 [İleride mini oyun balık dairesi/çember takip ve tıklama mantığı buraya eklenecek]

                    // 600 - 900 ms aralıklarla periyodik log
                    await Task.Delay(Random.Shared.Next(600, 901), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Görev iptal edildiğinde beklenen temiz çıkış
            }
            finally
            {
                BotLogger.LogInfo(clientInfo.Id, "Balık oyunu fonksiyonu sonlandırıldı.");
            }
        }
    }
}
