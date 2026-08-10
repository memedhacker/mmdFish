using Aether.Controls;
using Aether.Pages;
using Aether.States;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Aether.Forms
{
    public partial class MainForm : Form
    {
        private bool _isUpdatingSelectAll = false;

        // Sayfa ismi ile (Buton, Sayfa Üretici) ikilisini eşleyen harita
        private readonly Dictionary<string, (UIButton Button, Func<UserControl> ControlCreator)> _pageMap = new();

        // Merkezi ClientState üzerindeki seçili kart değişkeni (Tıklanan)
        public ClientCard? SelectedClient => ClientState.Instance.SelectedClient;

        // Merkezi ClientState üzerindeki işaretlenmiş kartlar değişkeni (Checkbox'lar)
        public List<ClientCard> CheckedClients => ClientState.Instance.CheckedClients;

        public MainForm()
        {
            InitializeComponent();
            RegisterPageButtons();

            // selectAllButton tıklama ve değer değişimi event abonelikleri
            selectAllButton.ValueChanged += SelectAllButton_ValueChanged;
            selectAllButton.Click += SelectAllButton_Click;

            // Panel yeniden boyutlandırıldığında yüklenen sayfaları yatayda stretch yap
            showPagePanel.Resize += ShowPagePanel_Resize;
        }

        /// <summary>
        /// İsmi 'p' ile başlayan tüm butonları dinamik olarak haritalandırır ve tıklama dinleyicilerini bağlar.
        /// </summary>
        private void RegisterPageButtons()
        {
            _pageMap.Clear();

            // Buton ve sayfa eşleşmeleri
            _pageMap["FishBot"] = (pFishBotButton, () => new FishBotPage());
            _pageMap["Puzzle"] = (pPuzzleButton, () => new FishPuzzlePage());
            _pageMap["Alchemy"] = (pAlchemyButton, () => new AlchemyPage());
            _pageMap["Upgrade"] = (pUpgradeButton, () => new UpgradePage());
            _pageMap["AntiBan"] = (pAntiBanButton, () => new AntiBanPage());

            // İsmi 'p' ile başlayan butonlara tıklama event'i bağlama
            foreach (var kvp in _pageMap)
            {
                string pageKey = kvp.Key;
                UIButton btn = kvp.Value.Button;

                btn.Click += (sender, e) =>
                {
                    NavigateToPage(pageKey);
                };
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Checkbox seçimleri değiştiğinde selectAllButton durumunu senkronize et
            ClientState.Instance.OnCheckedClientsChanged += ClientState_OnCheckedClientsChanged;

            // PageState değişim dinleyicisi
            PageState.Instance.OnPageChanged += PageState_OnPageChanged;

            // Program açıldığında pFishBotButton otomatik olarak disabled olsun ve FishBotPage yüklensin
            NavigateToPage("FishBot");
        }

        /// <summary>
        /// İlgili sayfayı showPagePanel içerisine yatayda sıfıra sıfır (stretch) şekilde ekler,
        /// aktif p-butonunu disable eder ve kalan tüm p-butonlarını enable yapar.
        /// </summary>
        /// <param name="pageKey">Sayfa anahtar kelimesi (Örn: "FishBot")</param>
        public void NavigateToPage(string pageKey)
        {
            if (!_pageMap.ContainsKey(pageKey)) return;

            // PageState güncellemesi
            if (PageState.Instance.CurrentPage != pageKey)
            {
                PageState.Instance.CurrentPage = pageKey;
            }

            // Buton durumlarını güncelle: Tıklanan buton disable, diğer tüm 'p' butonları enable
            foreach (var kvp in _pageMap)
            {
                bool isCurrent = kvp.Key.Equals(pageKey, StringComparison.OrdinalIgnoreCase);
                kvp.Value.Button.Enabled = !isCurrent;
            }

            // showPagePanel içerisini temizle ve yeni kontrolü sıfıra sıfır stretch şekilde ekle
            showPagePanel.SuspendLayout();
            showPagePanel.Controls.Clear();

            UserControl pageControl = _pageMap[pageKey].ControlCreator.Invoke();
            pageControl.Margin = new Padding(0);
            pageControl.Padding = new Padding(0);
            pageControl.Width = showPagePanel.ClientSize.Width;

            showPagePanel.Controls.Add(pageControl);
            showPagePanel.ResumeLayout(true);

            // CustomScrollBar'ı yeni sayfa içeriğiyle senkronize et
            pageScrollBar.SyncWithTarget();
        }

        private void ShowPagePanel_Resize(object? sender, EventArgs e)
        {
            showPagePanel.SuspendLayout();
            foreach (Control ctrl in showPagePanel.Controls)
            {
                ctrl.Margin = new Padding(0);
                ctrl.Width = showPagePanel.ClientSize.Width;
            }
            showPagePanel.ResumeLayout(true);
            pageScrollBar.SyncWithTarget();
        }

        private void PageState_OnPageChanged(object? sender, string pageKey)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => NavigateToPage(pageKey)));
            }
            else
            {
                NavigateToPage(pageKey);
            }
        }

        private void SelectAllButton_Click(object? sender, EventArgs e)
        {
            ToggleSelectAll(selectAllButton.Checked);
        }

        private void SelectAllButton_ValueChanged(object sender, bool value)
        {
            ToggleSelectAll(value);
        }

        private void ToggleSelectAll(bool isChecked)
        {
            if (_isUpdatingSelectAll) return;

            try
            {
                _isUpdatingSelectAll = true;
                clientsControl1.SetAllChecked(isChecked);
            }
            finally
            {
                _isUpdatingSelectAll = false;
            }
        }

        private void ClientState_OnCheckedClientsChanged(object? sender, IReadOnlyList<ClientCard> checkedList)
        {
            if (_isUpdatingSelectAll) return;

            _isUpdatingSelectAll = true;
            try
            {
                int totalClients = clientsControl1.TotalClientsCount;
                if (totalClients > 0 && checkedList.Count == totalClients)
                {
                    selectAllButton.Checked = true;
                }
                else
                {
                    selectAllButton.Checked = false;
                }
            }
            finally
            {
                _isUpdatingSelectAll = false;
            }
        }
    }
}
