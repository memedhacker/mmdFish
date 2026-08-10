using Aether.Controls;
using Aether.States;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Aether.Controls
{
    public partial class ClientsControl : UserControl
    {
        // Global ClientState üzerinden seçili olan kart ve işaretli kartlar listesi
        public ClientCard? SelectedClient => ClientState.Instance.SelectedClient;
        public List<ClientCard> CheckedClients => ClientState.Instance.CheckedClients;

        // Kart ve checkbox seçim event'leri
        public event EventHandler OnClientSelected;
        public event EventHandler OnCheckedClientsChanged;

        public ClientsControl()
        {
            InitializeComponent();
            LoadClients();
        }

        /// <summary>
        /// 10 adet ClientCard oluşturur ve FlowLayoutPanel içerisine ekler.
        /// Her kartın seçim olayını ve checkbox durumunu dinler.
        /// </summary>
        public void LoadClients()
        {
            clientListFlowPanel.Controls.Clear();

            var clientModels = Services.ClientService.Instance.GenerateDefaultClients(10);

            foreach (var clientModel in clientModels)
            {
                ClientCard card = new ClientCard();

                card.ClientName = clientModel.Name;
                card.ClientNumber = clientModel.Id;

                // Kart tıklandığında tetiklenecek event aboneliği
                card.OnCardSelected += ClientCard_OnCardSelected;

                // Checkbox değiştiğinde tetiklenecek event aboneliği
                card.OnCheckedChanged += (sender, e) => UpdateCheckedClients();

                // Mouse tekerleği kart üzerindeyken de scrollbar'ı çalıştırır
                AttachMouseWheel(card);

                clientListFlowPanel.Controls.Add(card);
            }

            // Varsayılan olarak 1. Client kartını otomatik olarak seç ve state/border rengini ayarla
            if (clientListFlowPanel.Controls.Count > 0 && clientListFlowPanel.Controls[0] is ClientCard firstCard)
            {
                SelectClient(firstCard);
            }

            UpdateCheckedClients();
            customScrollBar1.SyncWithTarget();
        }

        private void AttachMouseWheel(Control control)
        {
            control.MouseWheel += (sender, e) =>
            {
                int delta = e.Delta > 0 ? -60 : 60;
                customScrollBar1.Value += delta;
            };

            foreach (Control child in control.Controls)
            {
                AttachMouseWheel(child);
            }
        }

        /// <summary>
        /// Listedeki toplam ClientCard sayısını döner.
        /// </summary>
        public int TotalClientsCount => clientListFlowPanel.Controls.Count;

        /// <summary>
        /// Tüm client kartlarının checkbox durumunu belirler (Hepsini Seç / Kaldır).
        /// </summary>
        /// <param name="isChecked">True ise tümünü seçer, False ise kaldırır.</param>
        public void SetAllChecked(bool isChecked)
        {
            foreach (Control control in clientListFlowPanel.Controls)
            {
                if (control is ClientCard card)
                {
                    card.IsChecked = isChecked;
                }
            }

            UpdateCheckedClients();
        }

        /// <summary>
        /// Checkbox'ı işaretli olan kartları tarar ve ClientState üzerindeki CheckedClients listesini günceller.
        /// </summary>
        public void UpdateCheckedClients()
        {
            var checkedList = new List<ClientCard>();
            foreach (Control control in clientListFlowPanel.Controls)
            {
                if (control is ClientCard card && card.IsChecked)
                {
                    checkedList.Add(card);
                }
            }

            // State güncellemesi
            ClientState.Instance.UpdateCheckedClients(checkedList);
            OnCheckedClientsChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Kart tıklandığında çalışan event handler.
        /// </summary>
        private void ClientCard_OnCardSelected(object sender, EventArgs e)
        {
            if (sender is ClientCard selectedCard)
            {
                SelectClient(selectedCard);
            }
        }

        /// <summary>
        /// Seçilen kartı günceller ve önceki kartın seçim durumunu kaldırır.
        /// </summary>
        /// <param name="card">Seçilen ClientCard nesnesi</param>
        public void SelectClient(ClientCard card)
        {
            // Önceki seçili kart varsa seçim durumunu kaldır
            if (ClientState.Instance.SelectedClient != null)
            {
                ClientState.Instance.SelectedClient.IsSelected = false;
            }

            // ClientState üzerindeki seçili kartı güncelle
            ClientState.Instance.SelectedClient = card;

            // Seçilen kartın durumunu aktif yap
            if (ClientState.Instance.SelectedClient != null)
            {
                ClientState.Instance.SelectedClient.IsSelected = true;
            }

            // Seçim olayını dışarıya bildir
            OnClientSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}
