using Aether.Constants;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Aether.Helpers
{
    /// <summary>
    /// FishBot balık filtresi paneli için dinamik tablolar (Rare, Common, Others, DeadFishLoot) oluşturan yardımcı sınıf.
    /// </summary>
    public static class FishFilterTableBuilder
    {
        /// <summary>
        /// Balık Filtresi panelini temizler ve tüm alt tabloları dinamik olarak ekler.
        /// </summary>
        public static void BuildTables(Sunny.UI.UIPanel fishFilterPanel, Sunny.UI.UILine channelsLine, UserControl pageControl)
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

            currentY += 20;

            // 3. OTHERS TABLOSU (Diğer Öğeler - Sadece Yakala ve Yere At)
            Sunny.UI.UIPanel othersTablePanel = CreateOthersCategoryTable("Diğer Öğeler (Others)", "others", ref currentY);
            fishFilterPanel.Controls.Add(othersTablePanel);

            currentY += 20;

            // 4. DEAD FISH LOOT TABLOSU (Ölü Balık Ganimetleri - Sadece Yere At)
            Sunny.UI.UIPanel deadFishLootTablePanel = CreateSingleOptionCategoryTable("Ölü Balık Ganimetleri (Dead Fish Loot)", Path.Combine("others", "deadFishLoot"), "Yere At", Colors.PembeAcik, ref currentY);
            fishFilterPanel.Controls.Add(deadFishLootTablePanel);

            // fishFilterPanel ve sayfa boyutunu ayarla
            fishFilterPanel.Height = currentY + 30;
            pageControl.Size = new Size(pageControl.Width, channelsLine.Bottom + fishFilterPanel.Height + 50);
        }

        private static Sunny.UI.UIPanel CreateFishCategoryTable(string categoryTitle, string folderName, ref int currentY)
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

            const int rowHeight = 40;
            const int headerHeight = 45;
            const int titleHeight = 35;
            int totalHeight = titleHeight + headerHeight + (fishFiles.Length * rowHeight) + 15;

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

            Label colIconNameHeader = new Label
            {
                Text = "Balık Adı",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, tableTop + 8),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tableContainer.Controls.Add(colIconNameHeader);

            Label col1Header = new Label
            {
                Text = "Balığı Tut",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.YesilAcik,
                Location = new Point(210, tableTop + 8),
                Size = new Size(95, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col1Header);

            Label col2Header = new Label
            {
                Text = "Pişir",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 180, 0),
                Location = new Point(315, tableTop + 8),
                Size = new Size(95, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col2Header);

            Label col3Header = new Label
            {
                Text = "Öldür",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.MaviAcik,
                Location = new Point(420, tableTop + 8),
                Size = new Size(95, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col3Header);

            Label col4Header = new Label
            {
                Text = "Yere At",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.PembeAcik,
                Location = new Point(525, tableTop + 8),
                Size = new Size(95, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col4Header);

            Sunny.UI.UILine headerSeparator = new Sunny.UI.UILine
            {
                Location = new Point(15, tableTop + rowHeight - 2),
                Size = new Size(619, 2),
                LineColor = Color.FromArgb(60, 60, 65),
                FillColor = Color.Transparent
            };
            tableContainer.Controls.Add(headerSeparator);

            int yOffset = tableTop + rowHeight + 5;

            foreach (string filePath in fishFiles)
            {
                string rawFileName = Path.GetFileNameWithoutExtension(filePath);
                string formattedName = FormatFishName(rawFileName);

                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                };
                tableContainer.Controls.Add(pbIcon);

                Label lblFishName = new Label
                {
                    Text = formattedName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(55, yOffset + 7),
                    Size = new Size(150, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tableContainer.Controls.Add(lblFishName);

                Sunny.UI.UICheckBox chkCatch = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(245, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Colors.YesilAcik,
                    Checked = true
                };
                tableContainer.Controls.Add(chkCatch);

                Sunny.UI.UICheckBox chkCook = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(350, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Color.FromArgb(255, 180, 0),
                    Checked = false
                };
                tableContainer.Controls.Add(chkCook);

                Sunny.UI.UICheckBox chkKill = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(455, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = Colors.MaviAcik,
                    Checked = false
                };
                tableContainer.Controls.Add(chkKill);

                Sunny.UI.UICheckBox chkDrop = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(560, yOffset + 6),
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

        private static Sunny.UI.UIPanel CreateOthersCategoryTable(string categoryTitle, string folderName, ref int currentY)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishIcons", folderName);
            if (!Directory.Exists(assetPath))
            {
                string projectAssetPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "FishIcons", folderName);
                if (Directory.Exists(projectAssetPath))
                    assetPath = projectAssetPath;
            }

            string[] itemFiles = Directory.Exists(assetPath)
                ? Directory.GetFiles(assetPath, "*.png")
                : Array.Empty<string>();

            const int rowHeight = 40;
            const int headerHeight = 45;
            const int titleHeight = 35;
            int totalHeight = titleHeight + headerHeight + (itemFiles.Length * rowHeight) + 15;

            Color categoryColor = Colors.MaviAcik;
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

            Label colIconNameHeader = new Label
            {
                Text = "Öğe Adı",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, tableTop + 8),
                Size = new Size(220, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tableContainer.Controls.Add(colIconNameHeader);

            Label col1Header = new Label
            {
                Text = "Yakala",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.YesilAcik,
                Location = new Point(275, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col1Header);

            Label col2Header = new Label
            {
                Text = "Yere At",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Colors.PembeAcik,
                Location = new Point(515, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col2Header);

            Sunny.UI.UILine headerSeparator = new Sunny.UI.UILine
            {
                Location = new Point(15, tableTop + rowHeight - 2),
                Size = new Size(619, 2),
                LineColor = Color.FromArgb(60, 60, 65),
                FillColor = Color.Transparent
            };
            tableContainer.Controls.Add(headerSeparator);

            int yOffset = tableTop + rowHeight + 5;

            foreach (string filePath in itemFiles)
            {
                string rawFileName = Path.GetFileNameWithoutExtension(filePath);
                string formattedName = FormatFishName(rawFileName);

                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                };
                tableContainer.Controls.Add(pbIcon);

                Label lblItemName = new Label
                {
                    Text = formattedName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(60, yOffset + 7),
                    Size = new Size(200, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tableContainer.Controls.Add(lblItemName);

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

        private static Sunny.UI.UIPanel CreateSingleOptionCategoryTable(string categoryTitle, string relativeFolderPath, string optionHeader, Color optionColor, ref int currentY)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishIcons", relativeFolderPath);
            if (!Directory.Exists(assetPath))
            {
                string projectAssetPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "FishIcons", relativeFolderPath);
                if (Directory.Exists(projectAssetPath))
                    assetPath = projectAssetPath;
            }

            string[] itemFiles = Directory.Exists(assetPath)
                ? Directory.GetFiles(assetPath, "*.png")
                : Array.Empty<string>();

            const int rowHeight = 40;
            const int headerHeight = 45;
            const int titleHeight = 35;
            int totalHeight = titleHeight + headerHeight + (itemFiles.Length * rowHeight) + 15;

            Color categoryColor = Colors.PembeAcik;
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

            Label colIconNameHeader = new Label
            {
                Text = "Öğe Adı",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, tableTop + 8),
                Size = new Size(300, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            tableContainer.Controls.Add(colIconNameHeader);

            Label col1Header = new Label
            {
                Text = optionHeader,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = optionColor,
                Location = new Point(515, tableTop + 8),
                Size = new Size(110, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            tableContainer.Controls.Add(col1Header);

            Sunny.UI.UILine headerSeparator = new Sunny.UI.UILine
            {
                Location = new Point(15, tableTop + rowHeight - 2),
                Size = new Size(619, 2),
                LineColor = Color.FromArgb(60, 60, 65),
                FillColor = Color.Transparent
            };
            tableContainer.Controls.Add(headerSeparator);

            int yOffset = tableTop + rowHeight + 5;

            foreach (string filePath in itemFiles)
            {
                string rawFileName = Path.GetFileNameWithoutExtension(filePath);
                string formattedName = FormatFishName(rawFileName);

                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                };
                tableContainer.Controls.Add(pbIcon);

                Label lblItemName = new Label
                {
                    Text = formattedName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(60, yOffset + 7),
                    Size = new Size(250, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tableContainer.Controls.Add(lblItemName);

                Sunny.UI.UICheckBox chkOption = new Sunny.UI.UICheckBox
                {
                    Text = "",
                    Location = new Point(558, yOffset + 6),
                    Size = new Size(25, 25),
                    CheckBoxSize = 22,
                    CheckBoxColor = optionColor,
                    Checked = false
                };
                tableContainer.Controls.Add(chkOption);

                yOffset += rowHeight;
            }

            currentY += totalHeight;
            return tableContainer;
        }

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
