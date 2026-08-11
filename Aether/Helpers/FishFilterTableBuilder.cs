using Aether.Constants;
using Aether.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Aether.Helpers
{
    public record ColumnConfig(string HeaderText, string HeaderColor, int X, bool DefaultChecked);

    public record TableConfig(
        string Id,
        string Title,
        string FolderPath,
        string CategoryColor,
        string NameHeader,
        List<ColumnConfig> Columns
    );

    /// <summary>
    /// JSON konfigürasyonundan okuma yaparak FishBot paneli tablolarını tamamen jenerik inşa eden yardımcı sınıf.
    /// </summary>
    public static class FishFilterTableBuilder
    {
        private const int RowHeight = 40;
        private const int HeaderHeight = 45;
        private const int TitleHeight = 35;
        private const int PanelWidth = 649;

        public static void BuildTables(Sunny.UI.UIPanel fishFilterPanel, Sunny.UI.UILine channelsLine, UserControl pageControl)
        {
            fishFilterPanel.Controls.Clear();
            fishFilterPanel.BackColor = Colors.ArkaPlanKoyu;
            fishFilterPanel.FillColor = Colors.ArkaPlanKoyu;
            fishFilterPanel.FillColor2 = Colors.ArkaPlanKoyu;

            int currentY = 0;
            var configs = LoadConfigs();

            foreach (var cfg in configs)
            {
                var panel = CreateGenericTable(cfg, ref currentY);
                if (panel != null)
                {
                    fishFilterPanel.Controls.Add(panel);
                    currentY += 20;
                }
            }

            fishFilterPanel.Height = currentY + 10;
            pageControl.Size = new Size(pageControl.Width, channelsLine.Bottom + fishFilterPanel.Height + 50);
        }

        private static List<TableConfig> LoadConfigs()
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "fish_filter_config.json");
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "fish_filter_config.json");
            }

            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<TableConfig>>(json, options) ?? new List<TableConfig>();
            }

            return new List<TableConfig>();
        }

        /// <summary>
        /// Tüm istemciler başlangıçta oluşturulurken varsayılan balık filtresi state'ini doldurur.
        /// "Balığı Tut" / "Yakala" seçeneği hariç tüm seçenekler (Pişir, Öldür, Yere At) varsayılan olarak unchecked (false) olur.
        /// </summary>
        public static void PopulateDefaultFishFilter(FishBotSettings settings)
        {
            var configs = LoadConfigs();
            foreach (var cfg in configs)
            {
                string[] files = ResolveAssetFiles(cfg.FolderPath);
                foreach (string filePath in files)
                {
                    string itemKey = Path.GetFileNameWithoutExtension(filePath);
                    var filterItem = settings.GetOrCreateFilterItem(cfg.Id, itemKey);

                    foreach (var col in cfg.Columns)
                    {
                        bool isCatchCol = col.HeaderText == "Balığı Tut" || col.HeaderText == "Yakala";
                        filterItem.SetCheck(col.HeaderText, isCatchCol);
                    }
                }
            }
        }

        private static Sunny.UI.UIPanel? CreateGenericTable(TableConfig cfg, ref int currentY)
        {
            string[] files = ResolveAssetFiles(cfg.FolderPath);
            if (files.Length == 0) return null;

            int totalHeight = TitleHeight + HeaderHeight + (files.Length * RowHeight) + 15;
            Color categoryColor = ColorTranslator.FromHtml(cfg.CategoryColor);

            Sunny.UI.UIPanel container = new Sunny.UI.UIPanel
            {
                Location = new Point(0, currentY),
                Size = new Size(PanelWidth, totalHeight),
                BackColor = Colors.ArkaPlanAcik,
                FillColor = Colors.ArkaPlanAcik,
                FillColor2 = Colors.ArkaPlanAcik,
                RectColor = categoryColor,
                Radius = 15,
                Text = null
            };

            // Başlık
            container.Controls.Add(new Label
            {
                Text = cfg.Title,
                Font = new Font("Calibri", 14F, FontStyle.Bold),
                ForeColor = categoryColor,
                Location = new Point(15, 10),
                AutoSize = true
            });

            int tableTop = TitleHeight + 5;

            // Öğe / Balık Adı Sütun Başlığı
            container.Controls.Add(new Label
            {
                Text = cfg.NameHeader,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, tableTop + 8),
                Size = new Size(cfg.Columns.Count > 2 ? 180 : (cfg.Columns.Count > 1 ? 220 : 300), 25),
                TextAlign = ContentAlignment.MiddleLeft
            });

            // Sütun Başlıkları
            int headerSpacing = cfg.Columns.Count switch { 1 => 0, 2 => 240, _ => 105 };
            int headerStartX = cfg.Columns.Count switch { 1 => 515, 2 => 275, _ => 210 };

            for (int i = 0; i < cfg.Columns.Count; i++)
            {
                var col = cfg.Columns[i];
                container.Controls.Add(new Label
                {
                    Text = col.HeaderText,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = ColorTranslator.FromHtml(col.HeaderColor),
                    Location = new Point(headerStartX + (i * headerSpacing), tableTop + 8),
                    Size = new Size(cfg.Columns.Count > 2 ? 95 : 110, 25),
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            // Header Alt Çizgisi
            container.Controls.Add(new Sunny.UI.UILine
            {
                Location = new Point(15, tableTop + RowHeight - 2),
                Size = new Size(PanelWidth - 30, 2),
                LineColor = Colors.CizgiRengi,
                FillColor = Color.Transparent
            });

            // Satırlar
            int yOffset = tableTop + RowHeight + 5;
            foreach (string filePath in files)
            {
                string formattedName = FormatName(Path.GetFileNameWithoutExtension(filePath));

                // İkon
                container.Controls.Add(new PictureBox
                {
                    Location = new Point(20, yOffset + 4),
                    Size = new Size(30, 30),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(filePath)
                });

                // Adı
                container.Controls.Add(new Label
                {
                    Text = formattedName,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(55, yOffset + 7),
                    Size = new Size(cfg.Columns.Count > 2 ? 150 : (cfg.Columns.Count > 1 ? 200 : 250), 25),
                    TextAlign = ContentAlignment.MiddleLeft
                });

                // Checkboxlar
                Sunny.UI.UICheckBox? catchCheckBox = null;
                var otherCheckBoxes = new List<Sunny.UI.UICheckBox>();

                foreach (var col in cfg.Columns)
                {
                    var cb = new Sunny.UI.UICheckBox
                    {
                        Text = "",
                        Location = new Point(col.X, yOffset + 6),
                        Size = new Size(25, 25),
                        CheckBoxSize = 22,
                        CheckBoxColor = ColorTranslator.FromHtml(col.HeaderColor),
                        Checked = col.DefaultChecked,
                        // Binder'ın bu checkbox'ı tanımlayabilmesi için tag: "categoryId|itemKey|columnHeader"
                        Tag = $"{cfg.Id}|{Path.GetFileNameWithoutExtension(filePath)}|{col.HeaderText}"
                    };

                    container.Controls.Add(cb);

                    // İlk sütun (Balığı Tut veya Yakala) ilk kontrol olarak yakalanır
                    if (col.HeaderText == "Balığı Tut" || col.HeaderText == "Yakala")
                    {
                        catchCheckBox = cb;
                    }
                    else
                    {
                        otherCheckBoxes.Add(cb);
                    }
                }

                if (catchCheckBox != null && otherCheckBoxes.Count > 0)
                {
                    // Tut/Yakala durumuna göre diğerlerini enabled/disabled yapan yardımcı lambda
                    Action updateRowControls = () =>
                    {
                        bool isCatchChecked = catchCheckBox.Checked;
                        foreach (var cb in otherCheckBoxes)
                        {
                            if (!isCatchChecked && !FishBotPageBinder.IsBinding)
                            {
                                cb.Checked = false;
                            }
                            cb.Enabled = isCatchChecked;
                        }
                    };

                    catchCheckBox.ValueChanged += (s, value) => updateRowControls();
                    catchCheckBox.Click += (s, e) => updateRowControls();

                    // İlk yükleme esnasında başlangıç durumunu ayarla
                    updateRowControls();
                }

                yOffset += RowHeight;
            }

            currentY += totalHeight;
            return container;
        }

        private static string[] ResolveAssetFiles(string relativeFolderPath)
        {
            string assetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "FishIcons", relativeFolderPath);
            if (!Directory.Exists(assetPath))
            {
                string projectAssetPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Assets", "FishIcons", relativeFolderPath);
                if (Directory.Exists(projectAssetPath))
                    assetPath = projectAssetPath;
            }
            return Directory.Exists(assetPath) ? Directory.GetFiles(assetPath, "*.png") : Array.Empty<string>();
        }

        private static string FormatName(string rawName)
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
