using Aether.Models;
using Aether.States;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aether.Controls
{
    public partial class ClientsControl : UserControl
    {
        // Görsel seçim durumunu takip etmek için (UI katmanında tutulur, state'e taşınmaz)
        private ClientCard? _currentlySelectedCard;

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
            _currentlySelectedCard = null;

            var clientModels = Services.ClientService.GenerateDefaultClients(10);

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

            // Varsayılan olarak 1. Client kartını otomatik olarak seç
            if (clientListFlowPanel.Controls.Count > 0 && clientListFlowPanel.Controls[0] is ClientCard firstCard)
            {
                SelectClient(firstCard);
            }

            UpdateCheckedClients();
            customScrollBar1.SyncWithTarget();
        }

        private void AttachMouseWheel(System.Windows.Forms.Control control)
        {
            control.MouseWheel += (sender, e) =>
            {
                int delta = e.Delta > 0 ? -60 : 60;
                customScrollBar1.Value += delta;
            };

            foreach (System.Windows.Forms.Control child in control.Controls)
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
        public void SetAllChecked(bool isChecked)
        {
            foreach (System.Windows.Forms.Control control in clientListFlowPanel.Controls)
            {
                if (control is ClientCard card)
                {
                    card.IsChecked = isChecked;
                }
            }

            UpdateCheckedClients();
        }

        /// <summary>
        /// Checkbox'ı işaretli olan kartları tarar ve ClientState üzerindeki checked listesini günceller.
        /// ClientInfo (UI bağımsız) nesneleri oluşturarak state'e iletir.
        /// </summary>
        public void UpdateCheckedClients()
        {
            var checkedList = new List<ClientInfo>();
            foreach (System.Windows.Forms.Control control in clientListFlowPanel.Controls)
            {
                if (control is ClientCard card && card.IsChecked)
                {
                    checkedList.Add(new ClientInfo(card.ClientNumber, card.ClientName));
                }
            }

            ClientState.Instance.UpdateCheckedClients(checkedList);
        }

        /// <summary>
        /// Kart tıklandığında çalışan event handler.
        /// </summary>
        private void ClientCard_OnCardSelected(object sender, System.EventArgs e)
        {
            if (sender is ClientCard selectedCard)
            {
                SelectClient(selectedCard);
            }
        }

        /// <summary>
        /// Seçilen kartı günceller, görsel seçim durumunu yönetir ve
        /// ClientState'e UI bağımsız ClientInfo nesnesiyle seçimi bildirir.
        /// </summary>
        public void SelectClient(ClientCard card)
        {
            // Önceki seçili kartın görsel seçimini kaldır
            if (_currentlySelectedCard != null)
            {
                _currentlySelectedCard.IsSelected = false;
            }

            // Yeni kartı seç ve görsel durumunu güncelle
            _currentlySelectedCard = card;
            _currentlySelectedCard.IsSelected = true;

            // State katmanına UI bağımsız ClientInfo ilet
            ClientState.Instance.SelectedClient = new ClientInfo(card.ClientNumber, card.ClientName);
        }
    }
}
