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
        // UI veri yüklenirken olayların (ValueChanged) kaydetme tetiklemesini engellemek için bayrak
        private static bool _isBinding = false;

        public static bool IsBinding => _isBinding;

        /// <summary>
        /// Sayfadaki tüm kontrol değerlerini doğrular. Eğer geçersiz bir değer varsa hata mesajı gösterip 
        /// ekran kontrollerini önceki geçerli state'e geri yükler; geçerli ise settings nesnesine yazar.
        /// </summary>
        public static bool ValidateAndSaveToSettings(FishBotPage page, FishBotSettings settings)
        {
            if (_isBinding) return true;

            // 1. Min/Max Oltalama Hızı Denetimi
            if (!int.TryParse(page.MinFishSpeedTextBox.Text, out int minSpeed) ||
                !int.TryParse(page.MaxFishSpeedTextBox.Text, out int maxSpeed) ||
                minSpeed > maxSpeed)
            {
                ShowErrorAndRollback(page, settings, "Minimum oltalama hızı, maksimum oltalama hızından fazla olamaz.");
                return false;
            }

            // 2. Dakika Bazlı UpDownTextBox Denetimleri (5 - 200)
            int closeGameMin = page.CloseGameMinuteSelectUpDown.IntValue;
            if (closeGameMin < 5 || closeGameMin > 200)
            {
                ShowErrorAndRollback(page, settings, "Oyunu kapatma süresi (dakika) 5'ten az, 200'den fazla olamaz.");
                return false;
            }

            int charScreenMin = page.CharacterScreenUpDown.IntValue;
            if (charScreenMin < 5 || charScreenMin > 200)
            {
                ShowErrorAndRollback(page, settings, "Karakter ekranı süresi (dakika) 5'ten az, 200'den fazla olamaz.");
                return false;
            }

            int changeChanMin = page.ChangeChannelMinuteUpDown.IntValue;
            if (changeChanMin < 5 || changeChanMin > 200)
            {
                ShowErrorAndRollback(page, settings, "Kanal değiştirme süresi (dakika) 5'ten az, 200'den fazla olamaz.");
                return false;
            }

            // 3. Envanter Sayfası Denetimi (1 - 4)
            int invPage = page.InventoryPageSelectUpDown.IntValue;
            if (invPage < 1 || invPage > 4)
            {
                ShowErrorAndRollback(page, settings, "Envanter sayfası 1'den az, 4'ten fazla olamaz.");
                return false;
            }

            // Doğrulama başarılı: değerleri kaydet
            SaveGeneralSettings(page, settings);
            SaveChannelSettings(page, settings);
            SaveFishFilterSettings(page, settings);
            return true;
        }

        private static void ShowErrorAndRollback(FishBotPage page, FishBotSettings settings, string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ayar Değer Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            // Ekran kontrollerini önceki geçerli state verisine geri yükle
            LoadFromSettings(page, settings);
        }

        /// <summary>
        /// Sayfadaki tüm kontrol değerlerini verilen FishBotSettings nesnesine yazar.
        /// </summary>
        public static void SaveToSettings(FishBotPage page, FishBotSettings settings)
        {
            ValidateAndSaveToSettings(page, settings);
        }

        /// <summary>
        /// gameWindowSelectComboBox ve botSettingsNameTextBox HARİÇ tüm sayfa kontrollerine (checkbox, textbox, updown, switch)
        /// anlık değişiklik dinleyicileri bağlar. Herhangi bir değişiklikte o anki aktif client'ın state'ini anında günceller.
        /// </summary>
        public static void AttachRealtimeStateSync(FishBotPage page, Action getActiveClientSaveAction)
        {
            EventHandler onControlChanged = (s, e) =>
            {
                if (_isBinding) return;
                getActiveClientSaveAction();
            };

            // UIUpDownTextBox kontrolleri için tüm iç-dış değişiklik olaylarını (metin değişimi, artırma/azaltma butonları, mouse vb.) dinleyen yardımcı aksiyon
            Action<UIUpDownTextBox> bindUpDownEvents = (upDownCtrl) =>
            {
                upDownCtrl.Click += onControlChanged;
                upDownCtrl.TextChanged += onControlChanged;
                
                // Kontrolün içindeki alt kontrolleri (artırma/azaltma butonları, gizli textbox) da tara ve dinle
                foreach (Control subCtrl in upDownCtrl.Controls)
                {
                    subCtrl.Click += onControlChanged;
                    subCtrl.MouseUp += (s, e) => onControlChanged(s, e);
                    subCtrl.TextChanged += onControlChanged;
                }
            };

            // Genel Kontroller
            page.CloseGameCheckBox.Click += onControlChanged;
            page.CloseGameCheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            bindUpDownEvents(page.CloseGameMinuteSelectUpDown);

            page.CharacterScreenCheckBox.Click += onControlChanged;
            page.CharacterScreenCheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            bindUpDownEvents(page.CharacterScreenUpDown);

            page.BuyCampfireCheckBox.Click += onControlChanged;
            page.BuyCampfireCheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            page.BuyWormCheckbox.Click += onControlChanged;
            page.BuyWormCheckbox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            page.AnimationModeSwitch.Click += onControlChanged;
            page.AnimationModeSwitch.ActiveChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            bindUpDownEvents(page.InventoryPageSelectUpDown);

            page.MinFishSpeedTextBox.TextChanged += onControlChanged;
            page.MaxFishSpeedTextBox.TextChanged += onControlChanged;

            // Kanal Kontrolleri
            page.ChangeChannelCheckBox.Click += onControlChanged;
            page.ChangeChannelCheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            bindUpDownEvents(page.ChangeChannelMinuteUpDown);

            page.SelectAllChannelsCheckBox.Click += onControlChanged;
            page.SelectAllChannelsCheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            page.Ch1CheckBox.Click += onControlChanged;
            page.Ch1CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
            page.Ch2CheckBox.Click += onControlChanged;
            page.Ch2CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
            page.Ch3CheckBox.Click += onControlChanged;
            page.Ch3CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
            page.Ch4CheckBox.Click += onControlChanged;
            page.Ch4CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
            page.Ch5CheckBox.Click += onControlChanged;
            page.Ch5CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
            page.Ch6CheckBox.Click += onControlChanged;
            page.Ch6CheckBox.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);

            // Balık Filtre Tablosu Dinamik CheckBox'ları
            foreach (Control tablePanel in page.FishFilterPanel.Controls)
            {
                foreach (Control ctrl in tablePanel.Controls)
                {
                    if (ctrl is UICheckBox cb)
                    {
                        cb.Click += onControlChanged;
                        cb.ValueChanged += (s, val) => onControlChanged(s, EventArgs.Empty);
                    }
                }
            }
        }

        private static void SaveGeneralSettings(FishBotPage page, FishBotSettings settings)
        {
            settings.CloseGameEnabled = page.CloseGameCheckBox.Checked;
            settings.CloseGameAfterMinutes = page.CloseGameMinuteSelectUpDown.IntValue;

            settings.CharacterScreenEnabled = page.CharacterScreenCheckBox.Checked;
            settings.CharacterScreenAfterMinutes = page.CharacterScreenUpDown.IntValue;

            settings.BuyCampfireEnabled = page.BuyCampfireCheckBox.Checked;
            settings.BuyWormEnabled = page.BuyWormCheckbox.Checked;

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
                        if (parts.Length == 3 && parts[0] != "HEADER")
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
            try
            {
                _isBinding = true;
                LoadGeneralSettings(page, settings);
                LoadChannelSettings(page, settings);
                LoadFishFilterSettings(page, settings);
            }
            finally
            {
                _isBinding = false;
            }
        }

        private static void LoadGeneralSettings(FishBotPage page, FishBotSettings settings)
        {
            page.CloseGameCheckBox.Checked = settings.CloseGameEnabled;
            page.CloseGameMinuteSelectUpDown.IntValue = settings.CloseGameAfterMinutes;

            page.CharacterScreenCheckBox.Checked = settings.CharacterScreenEnabled;
            page.CharacterScreenUpDown.IntValue = settings.CharacterScreenAfterMinutes;

            page.BuyCampfireCheckBox.Checked = settings.BuyCampfireEnabled;
            page.BuyWormCheckbox.Checked = settings.BuyWormEnabled;

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
                // Her tablo panelinde birden fazla satır bulunur.
                // Satırları ayırt edebilmek için itemKey bazlı grupluyoruz.
                var rowCatchCheckBoxes = new System.Collections.Generic.Dictionary<string, UICheckBox>();
                var rowOtherCheckBoxes = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<UICheckBox>>();

                foreach (Control ctrl in tablePanel.Controls)
                {
                    if (ctrl is UICheckBox cb && cb.Tag is string tag)
                    {
                        var parts = tag.Split('|');
                        if (parts.Length == 3 && parts[0] != "HEADER")
                        {
                            string categoryId = parts[0];
                            string itemKey = parts[1];
                            string colHeader = parts[2];

                            if (settings.FishFilter.TryGetValue(categoryId, out var category) &&
                                category.TryGetValue(itemKey, out var item))
                            {
                                cb.Checked = item.GetCheck(colHeader, cb.Checked);
                            }

                            if (!rowOtherCheckBoxes.ContainsKey(itemKey))
                            {
                                rowOtherCheckBoxes[itemKey] = new System.Collections.Generic.List<UICheckBox>();
                            }

                            if (colHeader == "Balığı Tut" || colHeader == "Yakala")
                            {
                                rowCatchCheckBoxes[itemKey] = cb;
                            }
                            else
                            {
                                rowOtherCheckBoxes[itemKey].Add(cb);
                            }
                        }
                    }
                }

                // Her satırın tut kutusuna göre diğer kutularını güncelle
                foreach (var kvp in rowOtherCheckBoxes)
                {
                    string itemKey = kvp.Key;
                    var otherList = kvp.Value;

                    if (rowCatchCheckBoxes.TryGetValue(itemKey, out var catchCb))
                    {
                        bool isCatchChecked = catchCb.Checked;
                        foreach (var cb in otherList)
                        {
                            if (!isCatchChecked)
                            {
                                cb.Checked = false;
                            }
                            cb.Enabled = isCatchChecked;
                        }
                    }
                }
            }

            // Tüm tabloların başlık (header) checkbox'larını güncel satır durumlarına göre senkronize et
            UpdateAllHeaderCheckBoxes(page);
        }

        /// <summary>
        /// FishFilterPanel içerisindeki tüm tabloların başlık (HEADER) checkbox'larını
        /// altlarındaki satırların Checked durumuna göre (hepsi seçiliyse checked, biri bile değilse unchecked) günceller.
        /// </summary>
        public static void UpdateAllHeaderCheckBoxes(FishBotPage page)
        {
            foreach (Control tablePanel in page.FishFilterPanel.Controls)
            {
                var headerMap = new System.Collections.Generic.Dictionary<string, UICheckBox>();
                var rowMap = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<UICheckBox>>();

                foreach (Control ctrl in tablePanel.Controls)
                {
                    if (ctrl is UICheckBox cb && cb.Tag is string tag)
                    {
                        var parts = tag.Split('|');
                        if (parts.Length == 3)
                        {
                            string colHeader = parts[2];
                            if (parts[0] == "HEADER")
                            {
                                headerMap[colHeader] = cb;
                            }
                            else
                            {
                                if (!rowMap.ContainsKey(colHeader))
                                    rowMap[colHeader] = new System.Collections.Generic.List<UICheckBox>();
                                rowMap[colHeader].Add(cb);
                            }
                        }
                    }
                }

                foreach (var kvp in headerMap)
                {
                    string colHeader = kvp.Key;
                    var headerCb = kvp.Value;
                    if (rowMap.TryGetValue(colHeader, out var rows) && rows.Count > 0)
                    {
                        headerCb.Checked = rows.TrueForAll(r => r.Checked);
                    }
                }
            }
        }
    }
}
