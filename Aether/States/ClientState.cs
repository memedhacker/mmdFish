using Aether.Models;
using System;
using System.Collections.Generic;

namespace Aether.States
{
    /// <summary>
    /// Client kartlarının seçim durumlarını ve bot çalışma modüllerini merkezi olarak yöneten modüler state sınıfı.
    /// Program başlar başlamaz (Program.cs) tetiklenir ve hazırlanır.
    ///
    /// ÖNEMLİ: Bu sınıf yalnızca Aether.Models katmanına bağımlıdır.
    /// UI tipi olan ClientCard buraya girmez; bunun yerine ClientInfo kullanılır.
    /// </summary>
    public class ClientState
    {
        private static readonly Lazy<ClientState> _instance = new Lazy<ClientState>(() => new ClientState());

        /// <summary>
        /// Global tekil (Singleton) ClientState örneğine erişim noktası.
        /// </summary>
        public static ClientState Instance => _instance.Value;

        private ClientInfo? _selectedClient;
        private List<ClientInfo> _checkedClients = new List<ClientInfo>();

        // Bot modül durum değişkenleri (Default: false)
        private bool _isFishBotRunning = false;
        private bool _isUpgradeBotRunning = false;
        private bool _isFishPuzzleRunning = false;
        private bool _isAlchemyRunning = false;

        /// <summary> Tıklanarak seçilen client değiştiğinde tetiklenen olay. </summary>
        public event EventHandler<ClientInfo?>? OnSelectedClientChanged;

        /// <summary> Checkbox ile işaretlenen client'ların listesi değiştiğinde tetiklenen olay. </summary>
        public event EventHandler<IReadOnlyList<ClientInfo>>? OnCheckedClientsChanged;

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

        /// <summary> Tıklanarak seçilen aktif client (Tekil). </summary>
        public ClientInfo? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (!Equals(_selectedClient, value))
                {
                    _selectedClient = value;
                    OnSelectedClientChanged?.Invoke(this, _selectedClient);
                }
            }
        }

        /// <summary> Checkbox ile işaretlenmiş client'ların listesi (Çoklu). </summary>
        public List<ClientInfo> CheckedClients
        {
            get => _checkedClients;
            set
            {
                _checkedClients = value ?? new List<ClientInfo>();
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
        /// Checkbox ile işaretlenmiş client listesini yeniler ve dinleyicileri bilgilendirir.
        /// </summary>
        public void UpdateCheckedClients(IEnumerable<ClientInfo> clients)
        {
            _checkedClients = new List<ClientInfo>(clients);
            OnCheckedClientsChanged?.Invoke(this, _checkedClients.AsReadOnly());
        }
    }
}
