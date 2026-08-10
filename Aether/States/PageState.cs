using System;

namespace Aether.States
{
    /// <summary>
    /// Aktif sayfa durumunu (CurrentPage) ve sayfa geçişlerini merkezi olarak yöneten State sınıfı.
    /// </summary>
    public class PageState
    {
        private static readonly Lazy<PageState> _instance = new Lazy<PageState>(() => new PageState());

        /// <summary>
        /// Global tekil (Singleton) PageState örneğine erişim noktası.
        /// </summary>
        public static PageState Instance => _instance.Value;

        private string _currentPage = "FishBot";

        /// <summary>
        /// Aktif sayfa değiştiğinde tetiklenen olay.
        /// </summary>
        public event EventHandler<string>? OnPageChanged;

        /// <summary>
        /// Program ilk başladığında (Program.cs) PageState sistemini ayağa kaldırır.
        /// </summary>
        public static void Initialize()
        {
            var _ = Instance;
            Instance.Reset();
        }

        /// <summary>
        /// Varsayılan başlangıç sayfasına ("FishBot") sıfırlar.
        /// </summary>
        public void Reset()
        {
            _currentPage = "FishBot";
            OnPageChanged?.Invoke(this, _currentPage);
        }

        /// <summary>
        /// Aktif olarak görüntülenen sayfa ismi (Örn: "FishBot", "Puzzle", "Alchemy", "Upgrade", "AntiBan").
        /// </summary>
        public string CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value && !string.IsNullOrEmpty(value))
                {
                    _currentPage = value;
                    OnPageChanged?.Invoke(this, _currentPage);
                }
            }
        }
    }
}
