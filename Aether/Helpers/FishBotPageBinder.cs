using Aether.Models;
using Aether.Pages;
using Sunny.UI;
using System;
using System.Windows.Forms;

namespace Aether.Helpers
{
    /// <summary>
    /// FishBotPage UI kontrolleri ile FishBotSettings model nesnesi arasındaki
    /// çift yönlü veri bağlama işlemlerini yöneten yardımcı sınıf.
    /// Client değiştikçe mevcut ayarlar kaydedilir, yeni client'ın ayarları yüklenir.
    /// Checkbox'lar Tag tabanlı "categoryId|itemKey|columnHeader" sistemiyle okunur/yazılır.
    /// </summary>
    public static class FishBotPageBinder
    {
        // =================================================================
        // UI → Settings  (Sayfadan Modele Kaydet)
        // =================================================================

        /// <summary>
        /// Sayfadaki tüm kontrol değerlerini verilen FishBotSettings nesnesine yazar.
        /// </summary>
        public static void SaveToSettings(FishBotPage page, FishBotSettings settings)
        {
            SaveGeneralSettings(page, settings);
            SaveChannelSettings(page, settings);
            SaveFishFilterSettings(page, settings);
        }

        private static void SaveGeneralSettings(FishBotPage page, FishBotSettings settings)
        {
            settings.CloseGameEnabled = page.CloseGameCheckBox.Checked;
            settings.CloseGameAfterMinutes = page.CloseGameMinuteSelectUpDown.IntValue;

            settings.CharacterScreenEnabled = page.CharacterScreenCheckBox.Checked;
            settings.CharacterScreenAfterMinutes = page.CharacterScreenUpDown.IntValue;

            settings.BuyCampfireEnabled = page.BuyCampfireCheckBox.Checked;
            settings.CampfireCount = page.CampFireCountUpDown.IntValue;

            settings.BuyWormEnabled = page.BuyWormCheckbox.Checked;
            settings.WormCount = page.WormCountUpDown.IntValue;

            settings.AnimationMode = page.AnimationModeSwitch.Active ? "armor" : "mount";

            settings.InventoryPage = page.InventoryPageSelectUpDown.IntValue;

            if (int.TryParse(page.MinFishSpeedTextBox.Text, out int minMs))
                settings.FishingSpeedMinMs = minMs;
            if (int.TryParse(page.MaxFishSpeedTextBox.Text, out int maxMs))
                settings.FishingSpeedMaxMs = maxMs;
        }

        private static void SaveChannelSettings(FishBotPage page, FishBotSettings settings)
        {
            settings.ChangeChannelEnabled = page.ChangeChannelCheckBox.Checked;
            settings.ChangeChannelAfterMinutes = page.ChangeChannelMinuteUpDown.IntValue;
            settings.SelectAllChannels = page.SelectAllChannelsCheckBox.Checked;
            settings.Ch1 = page.Ch1CheckBox.Checked;
            settings.Ch2 = page.Ch2CheckBox.Checked;
            settings.Ch3 = page.Ch3CheckBox.Checked;
            settings.Ch4 = page.Ch4CheckBox.Checked;
            settings.Ch5 = page.Ch5CheckBox.Checked;
            settings.Ch6 = page.Ch6CheckBox.Checked;
        }

        /// <summary>
        /// FishFilterPanel içindeki dinamik UICheckBox'ların durumlarını settings'e yazar.
        /// Checkbox Tag formatı: "categoryId|itemKey|columnHeader"
        /// </summary>
        private static void SaveFishFilterSettings(FishBotPage page, FishBotSettings settings)
        {
            foreach (Control tablePanel in page.FishFilterPanel.Controls)
            {
                foreach (Control ctrl in tablePanel.Controls)
                {
                    if (ctrl is UICheckBox cb && cb.Tag is string tag)
                    {
                        var parts = tag.Split('|');
                        if (parts.Length == 3)
                        {
                            var item = settings.GetOrCreateFilterItem(parts[0], parts[1]);
                            item.SetCheck(parts[2], cb.Checked);
                        }
                    }
                }
            }
        }

        // =================================================================
        // Settings → UI  (Modelden Sayfaya Yükle)
        // =================================================================

        /// <summary>
        /// Verilen FishBotSettings nesnesindeki tüm değerleri sayfanın kontrollerine yazar.
        /// </summary>
        public static void LoadFromSettings(FishBotPage page, FishBotSettings settings)
        {
            LoadGeneralSettings(page, settings);
            LoadChannelSettings(page, settings);
            LoadFishFilterSettings(page, settings);
        }

        private static void LoadGeneralSettings(FishBotPage page, FishBotSettings settings)
        {
            page.CloseGameCheckBox.Checked = settings.CloseGameEnabled;
            page.CloseGameMinuteSelectUpDown.IntValue = settings.CloseGameAfterMinutes;

            page.CharacterScreenCheckBox.Checked = settings.CharacterScreenEnabled;
            page.CharacterScreenUpDown.IntValue = settings.CharacterScreenAfterMinutes;

            page.BuyCampfireCheckBox.Checked = settings.BuyCampfireEnabled;
            page.CampFireCountUpDown.IntValue = settings.CampfireCount;

            page.BuyWormCheckbox.Checked = settings.BuyWormEnabled;
            page.WormCountUpDown.IntValue = settings.WormCount;

            page.AnimationModeSwitch.Active = settings.AnimationMode == "armor";

            page.InventoryPageSelectUpDown.IntValue = settings.InventoryPage;

            page.MinFishSpeedTextBox.Text = settings.FishingSpeedMinMs.ToString();
            page.MaxFishSpeedTextBox.Text = settings.FishingSpeedMaxMs.ToString();
        }

        private static void LoadChannelSettings(FishBotPage page, FishBotSettings settings)
        {
            page.ChangeChannelCheckBox.Checked = settings.ChangeChannelEnabled;
            page.ChangeChannelMinuteUpDown.IntValue = settings.ChangeChannelAfterMinutes;
            page.SelectAllChannelsCheckBox.Checked = settings.SelectAllChannels;
            page.Ch1CheckBox.Checked = settings.Ch1;
            page.Ch2CheckBox.Checked = settings.Ch2;
            page.Ch3CheckBox.Checked = settings.Ch3;
            page.Ch4CheckBox.Checked = settings.Ch4;
            page.Ch5CheckBox.Checked = settings.Ch5;
            page.Ch6CheckBox.Checked = settings.Ch6;
        }

        /// <summary>
        /// FishFilterPanel içindeki UICheckBox'ları settings'ten okuyarak günceller.
        /// Tag formatı: "categoryId|itemKey|columnHeader"
        /// </summary>
        private static void LoadFishFilterSettings(FishBotPage page, FishBotSettings settings)
        {
            foreach (Control tablePanel in page.FishFilterPanel.Controls)
            {
                foreach (Control ctrl in tablePanel.Controls)
                {
                    if (ctrl is UICheckBox cb && cb.Tag is string tag)
                    {
                        var parts = tag.Split('|');
                        if (parts.Length == 3)
                        {
                            if (settings.FishFilter.TryGetValue(parts[0], out var category) &&
                                category.TryGetValue(parts[1], out var item))
                            {
                                cb.Checked = item.GetCheck(parts[2], cb.Checked);
                            }
                        }
                    }
                }
            }
        }
    }
}
