using Aether.Helpers;
using Aether.Models;
using Aether.States;
using System;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishBotPage : BaseBotPage
    {
        // Sayfa açıkken hangi client'ın ayarlarını gösterdiğimizi takip eder
        private int? _lastLoadedClientId = null;
        private System.Windows.Forms.Timer? _statusTimer;

        public FishBotPage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;

        // ------ Binder'ın erişmesi için internal property'ler ------
        internal Sunny.UI.UICheckBox CloseGameCheckBox => closeGameCheckBox;
        internal Sunny.UI.UIUpDownTextBox CloseGameMinuteSelectUpDown => closeGameMinuteSelectUpDown;
        internal Sunny.UI.UICheckBox CharacterScreenCheckBox => characterScreenCheckBox;
        internal Sunny.UI.UIUpDownTextBox CharacterScreenUpDown => characterScreenUpDown;
        internal Sunny.UI.UICheckBox BuyCampfireCheckBox => buyCampfireCheckBox;
        internal Sunny.UI.UICheckBox BuyWormCheckbox => buyWormCheckbox;
        internal Sunny.UI.UISwitch AnimationModeSwitch => animationModeSwitch;
        internal Sunny.UI.UIUpDownTextBox InventoryPageSelectUpDown => inventoryPageSelectUpDown;
        internal Sunny.UI.UITextBox MinFishSpeedTextBox => minFishSpeedTextBox;
        internal Sunny.UI.UITextBox MaxFishSpeedTextBox => maxFishSpeedTextBox;
        internal Sunny.UI.UICheckBox ChangeChannelCheckBox => changeChannelCheckBox;
        internal Sunny.UI.UIUpDownTextBox ChangeChannelMinuteUpDown => changeChannelMinuteUpDown;
        internal Sunny.UI.UICheckBox SelectAllChannelsCheckBox => selectAllChannelsCheckBox;
        internal Sunny.UI.UICheckBox Ch1CheckBox => ch1CheckBox;
        internal Sunny.UI.UICheckBox Ch2CheckBox => ch2CheckBox;
        internal Sunny.UI.UICheckBox Ch3CheckBox => ch3CheckBox;
        internal Sunny.UI.UICheckBox Ch4CheckBox => ch4CheckBox;
        internal Sunny.UI.UICheckBox Ch5CheckBox => ch5CheckBox;
        internal Sunny.UI.UICheckBox Ch6CheckBox => ch6CheckBox;
        internal Sunny.UI.UIPanel FishFilterPanel => fishFilterPanel;

        // ------ Preset UI kontrolleri için internal property'ler ------
        internal Sunny.UI.UITextBox BotSettingsNameTextBox => botSettingsNameTextBox;
        internal Sunny.UI.UIComboBox BotSettingsListComboBox => botSettingsListComboBox;

        protected override void OnLoad(EventArgs e)
        {
            // Base sınıf: client aboneliğini başlatır
            base.OnLoad(e);

            if (!DesignMode)
            {
                // Tabloları oluştur (Tag'ler burada atanır - Binder için zorunlu)
                FishFilterTableBuilder.BuildTables(fishFilterPanel, channelsLine, this);

                // metin2client.exe olan tüm görevleri ve HWND bilgilerini ComboBox'a yerleştir
                GameWindowProcessHelper.PopulateGameWindowComboBox(gameWindowSelectComboBox);

                // Sayfa açıldığında aktif client'ın ayarlarını yükle
                LoadSettingsForCurrentClient();

                // Sayfa açıldığında aktif seçili client'ın HWND bilgisini ComboBox ile senkronize et
                SyncComboBoxWithSelectedClient();

                // Canlı bot durumu ve çalışma süresi (fishBotTime / fishBotStatus) güncelleme timer'ı
                _statusTimer = new System.Windows.Forms.Timer();
                _statusTimer.Interval = 500;
                _statusTimer.Tick += (s, ev) => UpdateBotStatusAndTimerDisplay();
                _statusTimer.Start();

                // logPanel içerisine karanlık temalı RichTextBox log konsolunu yerleştir
                InitializeLogPanel();

                Services.FishBotService.Instance.OnFishBotStateChanged += FishBotService_OnFishBotStateChanged;
                UpdateBotStatusAndTimerDisplay();

                // Client değiştiğinde: önce mevcut ayarları kaydet, sonra yeni client'ın ayarlarını yükle
                ClientState.Instance.OnSelectedClientChanged += (s, clientInfo) =>
                {
                    SaveSettingsForLastClient();
                    LoadSettingsForCurrentClient();
                    SyncComboBoxWithSelectedClient();
                    UpdateBotStatusAndTimerDisplay();
                };

                // refreshClientListButton tıklanınca listeyi tekrar tara ve var olan seçimi korumaya çalış
                refresGameWindowList.Click += (s, ev) =>
                {
                    GameWindowProcessHelper.PopulateGameWindowComboBox(gameWindowSelectComboBox);
                    SyncComboBoxWithSelectedClient();
                };

                // highlightGameWindowButton tıklanınca seçili client penceresini en öne getir
                highlightGameWindowButton.Click += (s, ev) =>
                {
                    if (gameWindowSelectComboBox.SelectedItem is Aether.Models.GameWindowProcessInfo windowInfo)
                    {
                        GameWindowProcessHelper.BringWindowToFront(windowInfo.Handle);
                    }
                };

                // selectGameWindow butonuna basıldığında seçili olan client'ın state'ine HWND bağla
                selectGameWindow.Click += (s, ev) =>
                {
                    if (gameWindowSelectComboBox.SelectedItem is Aether.Models.GameWindowProcessInfo windowInfo)
                    {
                        var currentSelected = ClientState.Instance.SelectedClient;
                        int? currentId = currentSelected?.Id;

                        // Bu pencerenin (HWND) başka bir istemciye tanımlı olup olmadığını denetle
                        var existingOwner = ClientState.Instance.FindClientByHandle(windowInfo.Handle, currentId);

                        if (existingOwner != null)
                        {
                            // Çakışma var: Uyarı/Hata mesajı göster
                            MessageBox.Show(
                                $"Bu pencere zaten '{existingOwner.Name}' (Client #{existingOwner.Id})'ye tanımlanmış durumda.",
                                "Pencere Seçim Hatası",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                            return;
                        }

                        ClientState.Instance.UpdateSelectedClientHandle(windowInfo.Handle, windowInfo.ProcessId);
                    }
                    else
                    {
                        // '-- Seç --' varsayılan elemanı seçildiyse HWND ve PID'yi sıfırla
                        ClientState.Instance.UpdateSelectedClientHandle(IntPtr.Zero, 0);
                    }
                };

                // Kanal Değiştirme CheckBox mantıkları
                SetupChannelCheckBoxHandlers();

                // Sayfa açıldığında mevcut preset listesini ComboBox'a yükle
                RefreshPresetList();

                // addBotSettingsButton: mevcut ayarları preset olarak kaydet
                addBotSettingsButton.Click += (s, ev) => SavePreset();

                // loadBotSettingsButton: seçili preseti aktif client'a uygula
                loadBotSettingsButton.Click += (s, ev) => LoadSelectedPreset();

                // deleteBotSettingsButton: seçili preseti sil (onay sonrası)
                deleteBotSettingsButton.Click += (s, ev) => DeleteSelectedPreset();

                // Anlık kontrol değişikliklerini o anki aktif client state'ine bağla
                FishBotPageBinder.AttachRealtimeStateSync(this, SaveSettingsForLastClient);
            }
        }

        // -----------------------------------------------------------------
        // Per-Client Ayar Yükleme / Kaydetme
        // -----------------------------------------------------------------

        /// <summary>
        /// Seçili client için kayıtlı ayarları UI kontrollerine yükler.
        /// </summary>
        private void LoadSettingsForCurrentClient()
        {
            var client = ClientState.Instance.SelectedClient;
            int clientId = client?.Id ?? 1;

            _lastLoadedClientId = clientId;
            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientId);
            FishBotPageBinder.LoadFromSettings(this, settings);

            // Kanal enable/disable durumu yükleme sonrası da senkronize edilmeli
            SetChannelControlsEnabled(changeChannelCheckBox.Checked);
        }

        /// <summary>
        /// UI'daki mevcut değerleri, son yüklenen client'ın ayar kaydına yazar.
        /// </summary>
        private void SaveSettingsForLastClient()
        {
            if (_lastLoadedClientId == null) return;

            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(_lastLoadedClientId.Value);
            FishBotPageBinder.SaveToSettings(this, settings);
        }

        // -----------------------------------------------------------------
        // Kanal Checkbox Yardımcıları
        // -----------------------------------------------------------------

        private void SetupChannelCheckBoxHandlers()
        {
            // changeChannelCheckBox değiştiğinde altındaki tüm kanal checkbox'larını ve selectAllChannelsCheckBox'ı enabled/disabled yap
            changeChannelCheckBox.ValueChanged += (s, value) =>
            {
                SetChannelControlsEnabled(value);
            };
            changeChannelCheckBox.Click += (s, e) =>
            {
                SetChannelControlsEnabled(changeChannelCheckBox.Checked);
            };

            // selectAllChannelsCheckBox işaretlendiğinde/kaldırıldığında 6 kanal checkbox'ının durumunu değiştir
            selectAllChannelsCheckBox.ValueChanged += (s, value) =>
            {
                SetAllChannelsChecked(value);
            };
            selectAllChannelsCheckBox.Click += (s, e) =>
            {
                SetAllChannelsChecked(selectAllChannelsCheckBox.Checked);
            };

            // Başlangıç durumunu ayarla
            SetChannelControlsEnabled(changeChannelCheckBox.Checked);
        }

        internal void SetChannelControlsEnabled(bool enabled)
        {
            selectAllChannelsCheckBox.Enabled = enabled;
            ch1CheckBox.Enabled = enabled;
            ch2CheckBox.Enabled = enabled;
            ch3CheckBox.Enabled = enabled;
            ch4CheckBox.Enabled = enabled;
            ch5CheckBox.Enabled = enabled;
            ch6CheckBox.Enabled = enabled;
            changeChannelMinuteUpDown.Enabled = enabled;
        }

        private void SetAllChannelsChecked(bool isChecked)
        {
            ch1CheckBox.Checked = isChecked;
            ch2CheckBox.Checked = isChecked;
            ch3CheckBox.Checked = isChecked;
            ch4CheckBox.Checked = isChecked;
            ch5CheckBox.Checked = isChecked;
            ch6CheckBox.Checked = isChecked;
        }

        // -----------------------------------------------------------------
        // Preset Yönetimi (Kaydet / Yükle / Sil)
        // -----------------------------------------------------------------

        /// <summary>
        /// .mmdfishbot klasöründeki mevcut preset dosyalarını botSettingsListComboBox'a yükler.
        /// </summary>
        private void RefreshPresetList()
        {
            FishBotPresetManager.EnsureFolder();
            var names = FishBotPresetManager.GetPresetNames();

            botSettingsListComboBox.Items.Clear();
            foreach (var name in names)
                botSettingsListComboBox.Items.Add(name);

            if (botSettingsListComboBox.Items.Count > 0)
                botSettingsListComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// botSettingsNameTextBox'taki isimle sayfanın mevcut ayarlarını JSON olarak kaydeder.
        /// </summary>
        private void SavePreset()
        {
            string presetName = botSettingsNameTextBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(presetName))
            {
                MessageBox.Show(
                    "Lütfen bir preset ismi girin.",
                    "Kayıt Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var client = ClientState.Instance.SelectedClient;
            int clientId = client?.Id ?? (_lastLoadedClientId ?? 1);

            // Mevcut UI üzerindeki aktif değerleri seçili client'ın state'ine kaydet
            var currentSettings = FishBotSettingsRegistry.Instance.GetOrCreate(clientId);
            FishBotPageBinder.SaveToSettings(this, currentSettings);

            // Ayarların kopyasını alıp preset adını ekleyerek JSON olarak sakla
            string json = System.Text.Json.JsonSerializer.Serialize(currentSettings);
            var presetSettings = System.Text.Json.JsonSerializer.Deserialize<FishBotSettings>(json) ?? new FishBotSettings();
            presetSettings.SettingsName = presetName;

            FishBotPresetManager.SavePreset(presetName, presetSettings);
            RefreshPresetList();

            // ComboBox'ta yeni kayıtlı preseti seç
            int idx = botSettingsListComboBox.Items.IndexOf(presetName);
            if (idx >= 0) botSettingsListComboBox.SelectedIndex = idx;

            MessageBox.Show(
                $"'{presetName}' başarıyla kaydedildi.",
                "Preset Kaydedildi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// botSettingsListComboBox'ta seçili preseti okur, aktif client state'ini günceller ve UI'ya yansıtır.
        /// </summary>
        private void LoadSelectedPreset()
        {
            if (botSettingsListComboBox.SelectedItem is not string presetName)
            {
                MessageBox.Show(
                    "Lütfen yüklenecek bir preset seçin.",
                    "Yükleme Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var loaded = FishBotPresetManager.LoadPreset(presetName);
            if (loaded == null)
            {
                MessageBox.Show(
                    $"'{presetName}' preset dosyası okunamadı.",
                    "Yükleme Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var client = ClientState.Instance.SelectedClient;
            int clientId = client?.Id ?? (_lastLoadedClientId ?? 1);

            // Her istemcinin tamamen bağımsız bir state nesnesi olmasını garanti etmek için deep clone ile atıyoruz
            string json = System.Text.Json.JsonSerializer.Serialize(loaded);
            var clonedSettings = System.Text.Json.JsonSerializer.Deserialize<FishBotSettings>(json) ?? loaded;

            FishBotSettingsRegistry.Instance.Set(clientId, clonedSettings);
            _lastLoadedClientId = clientId;

            // UI'ya yansıt
            FishBotPageBinder.LoadFromSettings(this, clonedSettings);
            SetChannelControlsEnabled(changeChannelCheckBox.Checked);
        }

        /// <summary>
        /// Kullanıcıya onay penceresi göstererek seçili preseti siler ve listeyi yeniler.
        /// </summary>
        private void DeleteSelectedPreset()
        {
            if (botSettingsListComboBox.SelectedItem is not string presetName)
            {
                MessageBox.Show(
                    "Lütfen silinecek bir preset seçin.",
                    "Silme Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"'{presetName}' presetini kalıcı olarak silmek istediğinize emin misiniz?",
                "Preset Sil",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                FishBotPresetManager.DeletePreset(presetName);
                RefreshPresetList();
            }
        }

        // -----------------------------------------------------------------
        // GameWindow ComboBox Senkronizasyonu
        // -----------------------------------------------------------------

        private void SyncComboBoxWithSelectedClient()
        {
            var selectedClient = ClientState.Instance.SelectedClient;
            IntPtr currentHandle = selectedClient?.Handle ?? IntPtr.Zero;
            GameWindowProcessHelper.SelectMatchingHandleInComboBox(gameWindowSelectComboBox, currentHandle);
        }

        // -----------------------------------------------------------------
        // Bot Durumu ve Çalışma Süresi (fishBotTime & fishBotStatus) Yönetimi
        // -----------------------------------------------------------------

        private void FishBotService_OnFishBotStateChanged(object? sender, (int ClientId, bool IsRunning) e)
        {
            UpdateBotStatusAndTimerDisplay();
        }

        private void UpdateBotStatusAndTimerDisplay()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateBotStatusAndTimerDisplay));
                return;
            }

            var selectedClient = ClientState.Instance.SelectedClient;
            if (selectedClient == null)
            {
                fishBotStatus.Text = "Çalışmıyor";
                fishBotStatus.ForeColor = Color.Red;
                fishBotTime.Text = "00:00:00";
                return;
            }

            bool isRunning = Services.FishBotService.Instance.IsFishBotRunning(selectedClient.Id);
            if (isRunning)
            {
                TimeSpan elapsed = Services.FishBotService.Instance.GetBotElapsedTime(selectedClient.Id);
                fishBotStatus.Text = "Çalışıyor";
                fishBotStatus.ForeColor = Constants.Colors.YesilAcik;
                fishBotTime.Text = elapsed.ToString(@"hh\:mm\:ss");
            }
            else
            {
                fishBotStatus.Text = "Durdu";
                fishBotStatus.ForeColor = Color.Red;
                fishBotTime.Text = "00:00:00";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_statusTimer != null)
            {
                _statusTimer.Stop();
                _statusTimer.Dispose();
                _statusTimer = null;
            }
            Services.FishBotService.Instance.OnFishBotStateChanged -= FishBotService_OnFishBotStateChanged;
            Services.BotLogger.OnLog -= BotLogger_OnLog;
            base.OnHandleDestroyed(e);
        }

        #region LogPanel Yönetimi (Bot Logger UI)

        private RichTextBox? _rtbLogs;

        private void InitializeLogPanel()
        {
            _rtbLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 35),
                ForeColor = Color.FromArgb(220, 220, 225),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            logPanel.Controls.Clear();
            logPanel.Padding = new Padding(8);
            logPanel.Controls.Add(_rtbLogs);

            Services.BotLogger.OnLog += BotLogger_OnLog;
        }

        private int _logCounter = 0;

        private void BotLogger_OnLog(int clientId, string message, Color color)
        {
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(() => BotLogger_OnLog(clientId, message, color)));
                }
                catch { }
                return;
            }

            if (_rtbLogs == null || _rtbLogs.IsDisposed) return;

            // Her 15 logda bir veya satır sayısı 15'e ulaştığında log penceresini temizle
            _logCounter++;
            if (_logCounter > 15 || _rtbLogs.Lines.Length >= 15)
            {
                _rtbLogs.Clear();
                _logCounter = 1;
            }

            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{timeStamp}] [Client #{clientId}] {message}\n";

            _rtbLogs.SelectionStart = _rtbLogs.TextLength;
            _rtbLogs.SelectionLength = 0;
            _rtbLogs.SelectionColor = color;
            _rtbLogs.AppendText(line);
            _rtbLogs.SelectionColor = _rtbLogs.ForeColor;

            _rtbLogs.ScrollToCaret();
        }

        #endregion
    }
}
