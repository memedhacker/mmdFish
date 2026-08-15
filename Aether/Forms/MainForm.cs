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

        // Sayfa ismi ile (Buton, Sayfa Örneği) ikilisini eşleyen harita
        private readonly Dictionary<string, (UIButton Button, UserControl PageInstance)> _pageMap = new();

        public MainForm()
        {
            InitializeComponent();
            RegisterPageButtons();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;

            // CustomScrollBar'ı showPagePanel paneline bağla
            pageScrollBar.TargetControl = showPagePanel;

            // selectAllButton tıklama ve değer değişimi event abonelikleri
            selectAllButton.ValueChanged += SelectAllButton_ValueChanged;
            selectAllButton.Click += SelectAllButton_Click;

            // Panel yeniden boyutlandırıldığında yüklenen sayfaları yatayda stretch yap
            showPagePanel.Resize += ShowPagePanel_Resize;
        }

        /// <summary>
        /// İsmi 'p' ile başlayan tüm butonları dinamik olarak haritalandırır ve tekil sayfa örneklerini bağlar.
        /// </summary>
        private void RegisterPageButtons()
        {
            _pageMap.Clear();

            // Buton ve sayfa eşleşmeleri (Tekil nesneler olarak oluşturulur)
            _pageMap["Home"] = (null!, new HomePage());
            _pageMap["FishBot"] = (pFishBotButton, new FishBotPage());
            _pageMap["Puzzle"] = (pPuzzleButton, new FishPuzzlePage());
            _pageMap["Alchemy"] = (pAlchemyButton, new AlchemyPage());
            _pageMap["Upgrade"] = (pUpgradeButton, new UpgradePage());
            _pageMap["AntiBan"] = (pAntiBanButton, new AntiBanPage());

            foreach (var kvp in _pageMap)
            {
                string pageKey = kvp.Key;
                UIButton btn = kvp.Value.Button;

                if (btn != null)
                {
                    btn.Click += (sender, e) => NavigateToPage(pageKey);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Checkbox seçimleri değiştiğinde selectAllButton durumunu senkronize et
            ClientState.Instance.OnCheckedClientsChanged += ClientState_OnCheckedClientsChanged;

            // Program açıldığında Ana Sayfa (Home) otomatik yüklensin
            NavigateToPage("Home");
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
                if (kvp.Value.Button != null)
                {
                    bool isCurrent = kvp.Key.Equals(pageKey, StringComparison.OrdinalIgnoreCase);
                    kvp.Value.Button.Enabled = !isCurrent;
                }
            }

            // Panel içeriğini temizle ve yeni sayfayı ekle
            showPagePanel.SuspendLayout();
            showPagePanel.Controls.Clear();

            UserControl pageControl = _pageMap[pageKey].PageInstance;
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

        private void testButton_Click(object sender, EventArgs e)
        {
            /*
             * ====================================================================================================
             * 📘 İNTERAKTİF KOORDİNAT SEÇİM TESTİ VE BÖLGESEL YAKALAMA REHBERİ (TUTORIAL)
             * ====================================================================================================
             * 
             * 🔹 1. TEST PENCERESİ: PreviewFullWindowWithSelection(client.Handle, client.Name)
             *    - Seçili oyun penceresinin (kenarlıklar hariç) TÜM İÇ ALANINI (Client Area) çeker.
             *    - Açılan pencere üzerinde fareyle sürükleyerek istediğiniz kare/dikdörtgen alanı seçebilirsiniz.
             *    - Seçtiğiniz alanın (baslangic_x, baslangic_y, bitis_x, bitis_y) koordinatlarını ve hazır C# kodunu verir.
             * 
             * 🔹 2. KOD İÇİNDE KULLANIM: CaptureRegion(baslangic_x, baslangic_y, bitis_x, bitis_y)
             *    - Test penceresinden aldığınız koordinatları bu fonksiyona vererek bot döngüsünde doğrudan kullanın:
             * 
             *    Bitmap? bolgeResmi = Helpers.WindowRegionCaptureHelper.CaptureRegion(
             *        client.Handle,
             *        baslangic_x: 100,
             *        baslangic_y: 540,
             *        bitis_x: 450,
             *        bitis_y: 450);
             * 
             *    if (bolgeResmi != null)
             *    {
             *        // Template Matching ile arama yap:
             *        var sonuc = Constants.TemplateConstants.Match(bolgeResmi, Constants.TemplateConstants.Waypoints.BiseyTakildi, 0.85);
             *        if (sonuc.IsSuccess)
             *        {
             *            // Şablon bulundu! (sonuc.Location, sonuc.Confidence)
             *        }
             *    }
             * ====================================================================================================
             */

            // 1. Aktif seçili client ve HWND kontrolü
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero || !Native.Win32Native.IsWindow(client.Handle))
            {
                MessageBox.Show(
                    "Lütfen önce sol taraftaki listeden bir istemci (Client) seçin ve geçerli bir oyun penceresi (HWND) bağlı olduğundan emin olun.",
                    "İstemci Seçilmedi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // 2. Seçili HWND'nin tam iç görüntüsünü çek ve fare ile koordinat seçebileceğiniz test aracını aç
            var (success, message) = Helpers.WindowRegionCaptureHelper.PreviewFullWindowWithSelection(
                client.Handle,
                client.Name);

            if (!success)
            {
                MessageBox.Show(
                    message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #region F1 Acil Durdurma (Emergency Stop Hotkey)

        private const int HOTKEY_EMERGENCY_STOP_F1 = 9001;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Global F1 Acil Durdurma Kısayolunu Kaydet (Modifier tuşu olmadan: 0, VK_F1: 0x70)
            Native.Win32Native.RegisterHotKey(this.Handle, HOTKEY_EMERGENCY_STOP_F1, 0, Native.Win32Native.VK_F1);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Global F1 Kısayolunu Temizle
            Native.Win32Native.UnregisterHotKey(this.Handle, HOTKEY_EMERGENCY_STOP_F1);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.Win32Native.WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_EMERGENCY_STOP_F1)
            {
                // F1 Acil Durdurma Tetiklendi! Tüm çalışan botları anında durdur
                Services.FishBotService.Instance.StopAllBots();
                try
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
                catch { }

                System.Diagnostics.Debug.WriteLine("[MainForm] 🚨 F1 Acil Durdurma tuşuna basıldı! Tüm çalışan botlar anında durduruldu.");
            }

            base.WndProc(ref m);
        }

        #endregion
    }
}
