using Aether.Controls;
using System;
using System.Collections.Generic;

namespace Aether.States
{
    /// <summary>
    /// Client kartlarının seçim durumlarını ve bot çalışma modüllerini merkezi olarak yöneten modüler state sınıfı.
    /// Program başlar başlamaz (Program.cs) tetiklenir ve hazırlanır.
    /// </summary>
    public class ClientState
    {
        private static readonly Lazy<ClientState> _instance = new Lazy<ClientState>(() => new ClientState());

        /// <summary>
        /// Global tekil (Singleton) ClientState örneğine erişim noktası.
        /// </summary>
        public static ClientState Instance => _instance.Value;

        private ClientCard? _selectedClient;
        private List<ClientCard> _checkedClients = new List<ClientCard>();

        // Bot modül durum değişkenleri (Default: false)
        private bool _isFishBotRunning = false;
        private bool _isUpgradeBotRunning = false;
        private bool _isFishPuzzleRunning = false;
        private bool _isAlchemyRunning = false;

        /// <summary> Tıklanarak seçilen kart değiştiğinde tetiklenen olay. </summary>
        public event EventHandler<ClientCard?>? OnSelectedClientChanged;

        /// <summary> Checkbox ile işaretlenen kartların listesi değiştiğinde tetiklenen olay. </summary>
        public event EventHandler<IReadOnlyList<ClientCard>>? OnCheckedClientsChanged;

        /// <summary> Bot ve modül çalışma durumları değiştiğinde tetiklenen olay. </summary>
        public event EventHandler? OnBotStateChanged;

        /// <summary>
        /// Program ilk başladığında (Program.cs) State sistemini erkenden ayağa kaldırır.
        /// </summary>
        public static void Initialize()
        {
            var _ = Instance;
            Instance.Reset();
        }

        /// <summary>
        /// State durumlarını varsayılan değerlere sıfırlar ve event'leri tetikler.
        /// </summary>
        public void Reset()
        {
            _selectedClient = null;
            _checkedClients.Clear();
            _isFishBotRunning = false;
            _isUpgradeBotRunning = false;
            _isFishPuzzleRunning = false;
            _isAlchemyRunning = false;

            OnSelectedClientChanged?.Invoke(this, null);
            OnCheckedClientsChanged?.Invoke(this, _checkedClients.AsReadOnly());
            OnBotStateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary> Tıklanarak seçilen aktif kart (Tekil). </summary>
        public ClientCard? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnSelectedClientChanged?.Invoke(this, _selectedClient);
                }
            }
        }

        /// <summary> Checkbox ile işaretlenmiş kartların listesi (Çoklu). </summary>
        public List<ClientCard> CheckedClients
        {
            get => _checkedClients;
            set
            {
                _checkedClients = value ?? new List<ClientCard>();
                OnCheckedClientsChanged?.Invoke(this, _checkedClients.AsReadOnly());
            }
        }

        /// <summary> FishBot çalışma durumu (Default: false) </summary>
        public bool IsFishBotRunning
        {
            get => _isFishBotRunning;
            set
            {
                if (_isFishBotRunning != value)
                {
                    _isFishBotRunning = value;
                    OnBotStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary> UpgradeBot çalışma durumu (Default: false) </summary>
        public bool IsUpgradeBotRunning
        {
            get => _isUpgradeBotRunning;
            set
            {
                if (_isUpgradeBotRunning != value)
                {
                    _isUpgradeBotRunning = value;
                    OnBotStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary> FishPuzzle çalışma durumu (Default: false) </summary>
        public bool IsFishPuzzleRunning
        {
            get => _isFishPuzzleRunning;
            set
            {
                if (_isFishPuzzleRunning != value)
                {
                    _isFishPuzzleRunning = value;
                    OnBotStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary> Alchemy çalışma durumu (Default: false) </summary>
        public bool IsAlchemyRunning
        {
            get => _isAlchemyRunning;
            set
            {
                if (_isAlchemyRunning != value)
                {
                    _isAlchemyRunning = value;
                    OnBotStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Checkbox ile işaretlenmiş kartlar listesini yeniler ve dinleyicileri bilgilendirir.
        /// </summary>
        public void UpdateCheckedClients(IEnumerable<ClientCard> clients)
        {
            _checkedClients = new List<ClientCard>(clients);
            OnCheckedClientsChanged?.Invoke(this, _checkedClients.AsReadOnly());
        }
    }
}
