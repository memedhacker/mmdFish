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

            // ClientState'teki seçili client değiştikçe veya HWND güncellendikçe kart üzerindeki yazıyı güncelle
            ClientState.Instance.OnSelectedClientChanged += ClientState_OnSelectedClientChanged;

            // FishBotService çalışma durumları değiştikçe kart görsellerini güncelle
            Services.FishBotService.Instance.OnFishBotStateChanged += FishBotService_OnFishBotStateChanged;
        }

        private void FishBotService_OnFishBotStateChanged(object? sender, (int ClientId, bool IsRunning) e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new System.Action(() => FishBotService_OnFishBotStateChanged(sender, e)));
                return;
            }

            foreach (Control control in clientListFlowPanel.Controls)
            {
                if (control is ClientCard card && card.ClientNumber == e.ClientId)
                {
                    card.IsBotRunning = e.IsRunning;
                    break;
                }
            }
        }

        private void ClientState_OnSelectedClientChanged(object? sender, ClientInfo? clientInfo)
        {
            UpdateSelectedCardGameWindowText(clientInfo);
        }

        public void UpdateSelectedCardGameWindowText(ClientInfo? clientInfo)
        {
            if (_currentlySelectedCard == null) return;

            if (clientInfo != null && clientInfo.Handle != System.IntPtr.Zero && clientInfo.ProcessId != 0)
            {
                _currentlySelectedCard.GameWindowText = $"PID: {clientInfo.ProcessId}";
            }
            else if (clientInfo != null && clientInfo.Handle != System.IntPtr.Zero)
            {
                _currentlySelectedCard.GameWindowText = $"HWND: 0x{clientInfo.Handle.ToInt64():X}";
            }
            else
            {
                _currentlySelectedCard.GameWindowText = "Client Seçilmedi";
            }
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

                // State nesnesini erkenden oluşturalım/kaydedelim
                var clientInfo = ClientState.Instance.GetOrCreateClientInfo(card.ClientNumber, card.ClientName);

                // Kart tıklandığında tetiklenecek event aboneliği
                card.OnCardSelected += ClientCard_OnCardSelected;

                // Checkbox değiştiğinde tetiklenecek event aboneliği
                card.OnCheckedChanged += (sender, e) => UpdateCheckedClients();

                // startClient (Başlat/Durdur) butonuna tıklandığında balık botunu toggle et
                card.OnStartClientClicked += (sender, e) =>
                {
                    var targetClient = ClientState.Instance.GetOrCreateClientInfo(card.ClientNumber, card.ClientName);
                    var (success, message) = Services.FishBotService.Instance.ToggleFishBot(targetClient);

                    if (!success)
                    {
                        MessageBox.Show(
                            message,
                            "Pencere (HWND) Seçim Uyarısı",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                    }
                    else
                    {
                        // Bot başlatıldıysa (IsRunning ise) uygulamayı küçült
                        if (Services.FishBotService.Instance.IsFishBotRunning(targetClient.Id))
                        {
                            var parentForm = this.FindForm();
                            if (parentForm != null)
                            {
                                parentForm.WindowState = FormWindowState.Minimized;
                            }
                        }
                    }
                };

                // Mouse tekerleği kart üzerindeyken de scrollbar'ı çalıştırır
                AttachMouseWheel(card);

                // Kartın mevcut bot çalışma durumunu senkronize et
                card.IsBotRunning = Services.FishBotService.Instance.IsFishBotRunning(card.ClientNumber);

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
                    var clientInfo = ClientState.Instance.GetOrCreateClientInfo(card.ClientNumber, card.ClientName);
                    checkedList.Add(clientInfo);
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

            // State katmanında saklanan kalıcı nesneyi al
            var clientInfo = ClientState.Instance.GetOrCreateClientInfo(card.ClientNumber, card.ClientName);

            // Seçili kart üzerindeki HWND bilgisini tazele
            UpdateSelectedCardGameWindowText(clientInfo);

            // State katmanına duyur
            ClientState.Instance.SelectedClient = clientInfo;
        }
    }
}
