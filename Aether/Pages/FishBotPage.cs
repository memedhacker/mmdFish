using Aether.Constants;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishBotPage : BaseBotPage
    {
        public FishBotPage()
        {
            InitializeComponent();
        }

        protected override Label ClientNameLabel => clientNameLabel;

        protected override void OnLoad(EventArgs e)
        {
            // Base sınıf: client aboneliğini başlatır
            base.OnLoad(e);

            if (!DesignMode)
            {
                BuildFishFilterTable();
            }
        }

        private void BuildFishFilterTable()
        {
            fishFilterPanel.Controls.Clear();
            fishFilterPanel.BackColor = Color.FromArgb(24, 24, 27);
            fishFilterPanel.FillColor = Color.FromArgb(24, 24, 27);
            fishFilterPanel.FillColor2 = Color.FromArgb(24, 24, 27);

            int currentY = 0;

            // 1. RARE TABLOSU
            Sunny.UI.UIPanel rareTablePanel = CreateFishCategoryTable("Nadir Balıklar (Rare)", "rare", ref currentY);
            fishFilterPanel.Controls.Add(rareTablePanel);

            currentY += 20;

            // 2. COMMON TABLOSU
            Sunny.UI.UIPanel commonTablePanel = CreateFishCategoryTable("Yaygın Balıklar (Common)", "common", ref currentY);
            fishFilterPanel.Controls.Add(commonTablePanel);

            // fishFilterPanel toplam yüksekliğini ayarla
            fishFilterPanel.Height = currentY + 30;
            this.Size = new Size(this.Width, channelsLine.Bottom + fishFilterPanel.Height + 50);
        }

        private Sunny.UI.UIPanel CreateFishCategoryTable(string categoryTitle, string folderName, ref int currentY)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishIcons", folderName);
            if (!Directory.Exists(assetPath))
            {
                string projectAssetPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "FishIcons", folderName);
                if (Directory.Exists(projectAssetPath))
                    assetPath = projectAssetPath;
            }

            string[] fishFiles = Directory.Exists(assetPath)
                ? Directory.GetFiles(assetPath, "*.png")
                : Array.Empty<string>();

            // Layout sabitleri
            const int rowHeight = 40;
            const int headerHeight = 45;
            const int titleHeight = 35;
            int totalHeight = titleHeight + headerHeight + (fishFiles.Length * rowHeight) + 15;

            // Kategori rengi: rare → pembe, common → yeşil (Colors.cs sabitlerinden)
            Color categoryColor = folderName == "rare" ? Colors.PembeAcik : Colors.YesilAcik;
            Color panelBg = Color.FromArgb(30, 30, 35);

            Sunny.UI.UIPanel tableContainer = new Sunny.UI.UIPanel
            {
                Location = new Point(0, currentY),
                Size = new Size(649, totalHeight),
                BackColor = panelBg,
                FillColor = panelBg,
                FillColor2 = panelBg,
                RectColor = categoryColor,
                Radius = 15,
                Text = null
            };

            // Kategori Başlık Label
            Label lblCategory = new Label
            {
                Text = categoryTitle,
                Font = new Font("Calibri", 14F, FontStyle.Bold),
                ForeColor = categoryColor,
                Location = new Point(15, 10),
                AutoSize = true
            };
            tableContainer.Controls.Add(lblCategory);

            int tableTop = titleHeight + 5;

            // Tablo Header — sütun başlıkları
            Label colIconNameHeader = new Label
            {
                Text = "Balık Adı",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, tableTop + 8),
                Size = new Size(240, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tableContainer.Controls.Add(colIconNameHeader);

            Label col1Header = new Label
            {
                Text = "Balığı Tut",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.YesilAcik,
                Location = new Point(275, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col1Header);

            Label col2Header = new Label
            {
                Text = "Pişir",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(395, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col2Header);

            Label col3Header = new Label
            {
                Text = "Yere At",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.PembeAcik,
                Location = new Point(515, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col3Header);

            // Header Altı Çizgi
            Sunny.UI.UILine headerSeparator = new Sunny.UI.UILine
            {
                Location = new Point(15, tableTop + rowHeight - 2),
                Size = new Size(619, 2),
                LineColor = Color.FromArgb(60, 60, 65),
                FillColor = Color.Transparent
            };
            tableContainer.Controls.Add(headerSeparator);

            int yOffset = tableTop + rowHeight + 5;

            // Balık Satırları
            foreach (string filePath in fishFiles)
            {
                string rawFileName = Path.GetFileNameWithoutExtension(filePath);
                string formattedName = FormatFishName(rawFileName);

                // İkon
                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                };
                tableContainer.Controls.Add(pbIcon);

                // Balık Adı
                Label lblFishName = new Label
                {
                    Text = formattedName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(60, yOffset + 7),
                    Size = new Size(200, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tableContainer.Controls.Add(lblFishName);

                // Checkbox 1: Balığı Tut
                Sunny.UI.UICheckBox chkCatch = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(318, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Colors.YesilAcik,
                    Checked = true
                };
                tableContainer.Controls.Add(chkCatch);

                // Checkbox 2: Pişir
                Sunny.UI.UICheckBox chkCook = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(438, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Color.FromArgb(255, 180, 0),
                    Checked = false
                };
                tableContainer.Controls.Add(chkCook);

                // Checkbox 3: Yere At
                Sunny.UI.UICheckBox chkDrop = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(558, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Colors.PembeAcik,
                    Checked = false
                };
                tableContainer.Controls.Add(chkDrop);

                yOffset += rowHeight;
            }

            currentY += totalHeight;
            return tableContainer;
        }

        /// <summary>
        /// Dosya adını okunabilir balık ismine çevirir: "blue_fish" → "Blue Fish"
        /// </summary>
        private static string FormatFishName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return rawName;

            string clean = rawName.Replace('_', ' ');
            string[] words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
            }

            return string.Join(" ", words);
        }
    }
}
