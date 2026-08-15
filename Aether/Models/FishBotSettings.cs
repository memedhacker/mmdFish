using System.Collections.Generic;

namespace Aether.Models
{
    /// <summary>
    /// FishBotPage üzerindeki tüm kontrol değerlerinin per-client kalıcı state modelidir.
    /// fishbot_settings_example.json yapısını yansıtır.
    /// </summary>
    public class FishBotSettings
    {
        // --- Preset Adı ---
        public string SettingsName { get; set; } = string.Empty;

        // --- Oyundan Çık ---
        public bool CloseGameEnabled { get; set; } = false;
        public int CloseGameAfterMinutes { get; set; } = 25;

        // --- Kanal Değiştir ---
        public bool ChangeChannelEnabled { get; set; } = false;
        public int ChangeChannelAfterMinutes { get; set; } = 25;
        public bool SelectAllChannels { get; set; } = true;
        public bool Ch1 { get; set; } = true;
        public bool Ch2 { get; set; } = true;
        public bool Ch3 { get; set; } = true;
        public bool Ch4 { get; set; } = true;
        public bool Ch5 { get; set; } = true;
        public bool Ch6 { get; set; } = true;

        // --- Karakter At ---
        public bool CharacterScreenEnabled { get; set; } = false;
        public int CharacterScreenAfterMinutes { get; set; } = 25;

        // --- Kamp Ateşi ---
        public bool BuyCampfireEnabled { get; set; } = false;

        // --- Solucan ---
        public bool BuyWormEnabled { get; set; } = false;

        // --- Animasyon Modu ---
        /// <summary> "mount" = Binek Kullan | "armor" = Zırh Değiştir </summary>
        public string AnimationMode { get; set; } = "mount";

        // --- Envanter Sayfası ---
        public int InventoryPage { get; set; } = 1;

        // --- Oltalama Hızı ---
        public int FishingSpeedMinMs { get; set; } = 150;
        public int FishingSpeedMaxMs { get; set; } = 250;

        // --- Balık Filtresi ---
        // Anahtar: kategori ID'si (fish_filter_config.json'daki "id" alanı, örn: "rare", "common", "others", "deadFishLoot")
        // Değer: o kategorideki öğe state'leri (anahtar = dosya adı)
        public Dictionary<string, Dictionary<string, FishFilterItemState>> FishFilter { get; set; }
            = new Dictionary<string, Dictionary<string, FishFilterItemState>>();

        /// <summary>
        /// Belirtilen kategori ve öğe anahtarı için FishFilterItemState döner veya oluşturur.
        /// </summary>
        public FishFilterItemState GetOrCreateFilterItem(string categoryId, string itemKey)
        {
            if (!FishFilter.TryGetValue(categoryId, out var category))
            {
                category = new Dictionary<string, FishFilterItemState>();
                FishFilter[categoryId] = category;
            }

            if (!category.TryGetValue(itemKey, out var item))
            {
                item = new FishFilterItemState(itemKey);
                category[itemKey] = item;
            }

            return item;
        }
    }
}
