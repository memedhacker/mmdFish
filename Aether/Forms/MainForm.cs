using Aether.Controls;
using Aether.Models;
using Aether.Pages;
using Aether.States;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Aether.Forms
{
    public partial class MainForm : Form
    {
        private bool _isUpdatingSelectAll = false;

        // Sayfa ismi ile (Buton, Sayfa Üretici) ikilisini eşleyen harita
        private readonly Dictionary<string, (UIButton Button, Func<UserControl> ControlCreator)> _pageMap = new();

        public MainForm()
        {
            InitializeComponent();
            RegisterPageButtons();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
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
            _pageMap["FishBot"]  = (pFishBotButton,  () => new FishBotPage());
            _pageMap["Puzzle"]   = (pPuzzleButton,   () => new FishPuzzlePage());
            _pageMap["Alchemy"]  = (pAlchemyButton,  () => new AlchemyPage());
            _pageMap["Upgrade"]  = (pUpgradeButton,  () => new UpgradePage());
            _pageMap["AntiBan"]  = (pAntiBanButton,  () => new AntiBanPage());

            foreach (var kvp in _pageMap)
            {
                string pageKey = kvp.Key;
                UIButton btn = kvp.Value.Button;

                btn.Click += (sender, e) => NavigateToPage(pageKey);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Checkbox seçimleri değiştiğinde selectAllButton durumunu senkronize et
            ClientState.Instance.OnCheckedClientsChanged += ClientState_OnCheckedClientsChanged;

            // Program açıldığında FishBotPage otomatik yüklensin
            NavigateToPage("FishBot");
        }

        /// <summary>
        /// İlgili sayfayı showPagePanel içerisine yatayda stretch şekilde ekler,
        /// aktif p-butonunu disable eder ve kalan tüm p-butonlarını enable yapar.
        /// </summary>
        public void NavigateToPage(string pageKey)
        {
            if (!_pageMap.ContainsKey(pageKey)) return;

            // PageState'i senkron tut (harici okumalar için)
            if (PageState.Instance.CurrentPage != pageKey)
            {
                PageState.Instance.CurrentPage = pageKey;
            }

            // Buton durumlarını güncelle
            foreach (var kvp in _pageMap)
            {
                bool isCurrent = kvp.Key.Equals(pageKey, StringComparison.OrdinalIgnoreCase);
                kvp.Value.Button.Enabled = !isCurrent;
            }

            // Panel içeriğini temizle ve yeni sayfayı ekle
            showPagePanel.SuspendLayout();
            showPagePanel.Controls.Clear();

            UserControl pageControl = _pageMap[pageKey].ControlCreator.Invoke();
            pageControl.Margin = new System.Windows.Forms.Padding(0);
            pageControl.Padding = new System.Windows.Forms.Padding(0);
            pageControl.Width = showPagePanel.ClientSize.Width;

            showPagePanel.Controls.Add(pageControl);
            showPagePanel.ResumeLayout(true);

            pageScrollBar.SyncWithTarget();
        }

        private void ShowPagePanel_Resize(object? sender, EventArgs e)
        {
            showPagePanel.SuspendLayout();
            foreach (System.Windows.Forms.Control ctrl in showPagePanel.Controls)
            {
                ctrl.Margin = new System.Windows.Forms.Padding(0);
                ctrl.Width = showPagePanel.ClientSize.Width;
            }
            showPagePanel.ResumeLayout(true);
            pageScrollBar.SyncWithTarget();
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

        private void ClientState_OnCheckedClientsChanged(object? sender, IReadOnlyList<ClientInfo> checkedList)
        {
            if (_isUpdatingSelectAll) return;

            _isUpdatingSelectAll = true;
            try
            {
                int totalClients = clientsControl1.TotalClientsCount;
                selectAllButton.Checked = totalClients > 0 && checkedList.Count == totalClients;
            }
            finally
            {
                _isUpdatingSelectAll = false;
            }
        }
    }
}
