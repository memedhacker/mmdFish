using Aether.Controls;
using Aether.States;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishBotPage : UserControl
    {
        public FishBotPage()
        {
            InitializeComponent();
            UpdateSelectedClientDisplay(ClientState.Instance.SelectedClient);

            // State değişimini canlı olarak dinle
            ClientState.Instance.OnSelectedClientChanged += ClientState_OnSelectedClientChanged;

            // Balık Filtre Tablosunu Oluştur
            BuildFishFilterTable();
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
            string assetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishIcons", folderName);
            if (!System.IO.Directory.Exists(assetPath))
            {
                // Fallback project path check if running under bin/Debug
                string projectAssetPath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "FishIcons", folderName);
                if (System.IO.Directory.Exists(projectAssetPath))
                {
                    assetPath = projectAssetPath;
                }
            }

            string[] fishFiles = System.IO.Directory.Exists(assetPath) 
                ? System.IO.Directory.GetFiles(assetPath, "*.png") 
                : Array.Empty<string>();

            int rowHeight = 40;
            int headerHeight = 45;
            int titleHeight = 35;
            int totalHeight = titleHeight + headerHeight + (fishFiles.Length * rowHeight) + 15;

            Sunny.UI.UIPanel tableContainer = new Sunny.UI.UIPanel
            {
                Location = new Point(0, currentY),
                Size = new Size(649, totalHeight),
                BackColor = Color.FromArgb(30, 30, 35),
                FillColor = Color.FromArgb(30, 30, 35),
                FillColor2 = Color.FromArgb(30, 30, 35),
                RectColor = folderName == "rare" ? Color.FromArgb(255, 139, 164) : Color.FromArgb(135, 193, 109),
                Radius = 15,
                Text = null
            };

            // Kategori Başlık Label
            Label lblCategory = new Label
            {
                Text = categoryTitle,
                Font = new Font("Calibri", 14F, FontStyle.Bold),
                ForeColor = folderName == "rare" ? Color.FromArgb(255, 139, 164) : Color.FromArgb(135, 193, 109),
                Location = new Point(15, 10),
                AutoSize = true
            };
            tableContainer.Controls.Add(lblCategory);

            int tableTop = titleHeight + 5;

            // Tablo Header
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
                ForeColor = Color.FromArgb(135, 193, 109),
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
                ForeColor = Color.FromArgb(255, 139, 164),
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
                string rawFileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string formattedName = FormatFishName(rawFileName);

                // Icon
                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                };
                tableContainer.Controls.Add(pbIcon);

                // Name
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
                    CheckBoxColor = Color.FromArgb(135, 193, 109),
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
                    CheckBoxColor = Color.FromArgb(255, 139, 164),
                    Checked = false
                };
                tableContainer.Controls.Add(chkDrop);

                yOffset += rowHeight;
            }

            currentY += totalHeight;
            return tableContainer;
        }

        private string FormatFishName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName)) return rawName;

            // Alt tireleri boşluk yap ve her kelimenin ilk harfini büyük yap
            string clean = rawName.Replace('_', ' ');
            string[] words = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        private void ClientState_OnSelectedClientChanged(object? sender, ClientCard? selectedCard)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateSelectedClientDisplay(selectedCard)));
            }
            else
            {
                UpdateSelectedClientDisplay(selectedCard);
            }
        }

        private void UpdateSelectedClientDisplay(ClientCard? selectedCard)
        {
            if (selectedCard != null && !string.IsNullOrEmpty(selectedCard.ClientName))
            {
                clientNameLabel.Text = selectedCard.ClientName;
            }
            else
            {
                clientNameLabel.Text = "Seçim Yok";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Event aboneliğini güvenle kaldır
            ClientState.Instance.OnSelectedClientChanged -= ClientState_OnSelectedClientChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
