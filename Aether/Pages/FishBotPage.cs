using Aether.Helpers;
using System;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishBotPage : BaseBotPage
    {
        public FishBotPage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;

        protected override void OnLoad(EventArgs e)
        {
            // Base sınıf: client aboneliğini başlatır
            base.OnLoad(e);

            if (!DesignMode)
            {
                // Tabloları oluştur
                FishFilterTableBuilder.BuildTables(fishFilterPanel, channelsLine, this);

                // metin2client.exe olan tüm görevleri ve HWND bilgilerini ComboBox'a yerleştir
                GameWindowProcessHelper.PopulateGameWindowComboBox(gameWindowSelectComboBox);

                // Sayfa açıldığında aktif seçili client'ın HWND bilgisini ComboBox ile senkronize et
                SyncComboBoxWithSelectedClient();

                // Sol taraftaki kartlardan herhangi biri tıklandığında/seçildiğinde ComboBox seçimini güncelle
                Aether.States.ClientState.Instance.OnSelectedClientChanged += (s, clientInfo) =>
                {
                    SyncComboBoxWithSelectedClient();
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
                        var currentSelected = Aether.States.ClientState.Instance.SelectedClient;
                        int? currentId = currentSelected?.Id;

                        // Bu pencerenin (HWND) başka bir istemciye tanımlı olup olmadığını denetle
                        var existingOwner = Aether.States.ClientState.Instance.FindClientByHandle(windowInfo.Handle, currentId);

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

                        Aether.States.ClientState.Instance.UpdateSelectedClientHandle(windowInfo.Handle, windowInfo.ProcessId);
                    }
                    else
                    {
                        // '-- Seç --' varsayılan elemanı seçildiyse HWND ve PID'yi sıfırla
                        Aether.States.ClientState.Instance.UpdateSelectedClientHandle(IntPtr.Zero, 0);
                    }
                };

                // Kanal Değiştirme CheckBox mantıkları
                SetupChannelCheckBoxHandlers();
            }
        }

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

        private void SetChannelControlsEnabled(bool enabled)
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

        private void SyncComboBoxWithSelectedClient()
        {
            var selectedClient = Aether.States.ClientState.Instance.SelectedClient;
            IntPtr currentHandle = selectedClient?.Handle ?? IntPtr.Zero;
            GameWindowProcessHelper.SelectMatchingHandleInComboBox(gameWindowSelectComboBox, currentHandle);
        }

       
    }
}
