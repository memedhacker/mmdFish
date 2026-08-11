using Aether.Models;
using System.Collections.Generic;

namespace Aether.States
{
    /// <summary>
    /// Her bir client (ID bazlı) için ayrı FishBotSettings nesnelerini tutan singleton kayıt defteri.
    /// ClientState gibi program boyunca yaşar; client değiştiğinde sayfanın verileri kaybolmaz.
    /// </summary>
    public class FishBotSettingsRegistry
    {
        private static readonly System.Lazy<FishBotSettingsRegistry> _instance
            = new System.Lazy<FishBotSettingsRegistry>(() => new FishBotSettingsRegistry());

        /// <summary> Global tekil FishBotSettingsRegistry örneğine erişim noktası. </summary>
        public static FishBotSettingsRegistry Instance => _instance.Value;

        // Client ID → FishBotSettings
        private readonly Dictionary<int, FishBotSettings> _settingsMap
            = new Dictionary<int, FishBotSettings>();

        private FishBotSettingsRegistry() { }

        /// <summary>
        /// Belirtilen client ID'si için kayıtlı FishBotSettings nesnesini döner.
        /// Eğer bu client için henüz ayar oluşturulmamışsa varsayılan değerlerle yeni bir nesne oluşturur.
        /// </summary>
        public FishBotSettings GetOrCreate(int clientId)
        {
            if (!_settingsMap.TryGetValue(clientId, out var settings))
            {
                settings = new FishBotSettings();
                Helpers.FishFilterTableBuilder.PopulateDefaultFishFilter(settings);
                _settingsMap[clientId] = settings;
            }

            return settings;
        }

        /// <summary>
        /// Belirtilen client ID'sine ait ayarların zaten kayıtlı olup olmadığını döner.
        /// </summary>
        public bool HasSettings(int clientId) => _settingsMap.ContainsKey(clientId);

        /// <summary>
        /// Belirtilen client ID'sine ait ayarları tamamen sıfırlar (varsayılan değerlere döner).
        /// </summary>
        public void Reset(int clientId)
        {
            var settings = new FishBotSettings();
            Helpers.FishFilterTableBuilder.PopulateDefaultFishFilter(settings);
            _settingsMap[clientId] = settings;
        }

        /// <summary>
        /// Belirtilen client ID'si için var olan kayıt yerine dışarıdan sağlanan settings nesnesini atar.
        /// Preset yükleme işlemlerinde kullanılır.
        /// </summary>
        public void Set(int clientId, FishBotSettings settings)
        {
            _settingsMap[clientId] = settings;
        }

        /// <summary>
        /// Program başlatıldığında tüm istemciler (varsayılan 10 adet) için bağımsız varsayılan ayar state'lerini oluşturur.
        /// </summary>
        public void InitializeDefaults(int clientCount = 10)
        {
            for (int id = 1; id <= clientCount; id++)
            {
                if (!_settingsMap.ContainsKey(id))
                {
                    var settings = new FishBotSettings();
                    Helpers.FishFilterTableBuilder.PopulateDefaultFishFilter(settings);
                    _settingsMap[id] = settings;
                }
            }
        }

        /// <summary>
        /// Tüm client ayarlarını temizler.
        /// </summary>
        public void ResetAll()
        {
            _settingsMap.Clear();
        }
    }
}
