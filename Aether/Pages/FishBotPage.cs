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
                ClientProcessHelper.PopulateClientComboBox(clientSelectComboBox);

                // refreshClientListButton tıklanınca listeyi tekrar tara ve yenile
                refreshClientListButton.Click += (s, ev) =>
                {
                    ClientProcessHelper.PopulateClientComboBox(clientSelectComboBox);
                };

                // highlightClientButton tıklanınca seçili client penceresini en öne getir
                highlightClientButton.Click += (s, ev) =>
                {
                    if (clientSelectComboBox.SelectedItem is Aether.Models.ClientProcessInfo clientInfo)
                    {
                        ClientProcessHelper.BringWindowToFront(clientInfo.Handle);
                    }
                };
            }
        }
    }
}
