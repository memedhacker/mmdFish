using Aether.Controls;
using Aether.States;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class UpgradePage : UserControl
    {
        public UpgradePage()
        {
            InitializeComponent();
            UpdateSelectedClientDisplay(ClientState.Instance.SelectedClient);

            // State değişimini canlı olarak dinle
            ClientState.Instance.OnSelectedClientChanged += ClientState_OnSelectedClientChanged;
        }

        private void ClientState_OnSelectedClientChanged(object? sender, ClientCard? selectedCard)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateSelectedClientDisplay(selectedCard)));
            }
            else
            {
                UpdateSelectedClientDisplay(selectedCard);
            }
        }

        private void UpdateSelectedClientDisplay(ClientCard? selectedCard)
        {
            if (selectedCard != null && !string.IsNullOrEmpty(selectedCard.ClientName))
            {
                clientNameLabel.Text = selectedCard.ClientName;
            }
            else
            {
                clientNameLabel.Text = "Seçim Yok";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ClientState.Instance.OnSelectedClientChanged -= ClientState_OnSelectedClientChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
