using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using Aether.States;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishPuzzlePage : BaseBotPage
    {
        private readonly Panel[] _slotPanels;

        public FishPuzzlePage()
        {
            InitializeComponent();

            _slotPanels = new Panel[]
            {
                slot1, slot2, slot3, slot4, slot5, slot6,
                slot7, slot8, slot9, slot10, slot11, slot12,
                slot13, slot14, slot15, slot16, slot17, slot18,
                slot19, slot20, slot21, slot22, slot23, slot24
            };

            InitializeLogPanel();

            getSlotColorsButton.Click += GetSlotColorsButton_Click;
            selectNewPuzzlePart.Click += SelectNewPuzzlePart_Click;
            dropPuzzlePart.Click += DropPuzzlePart_Click;
        }

        protected override Label ClientNameLabel => clientNameLabel;

        /// <summary>
        /// "Slotları Çek" butonuna tıklandığında seçili istemcinin ekranından
        /// PuzzleGameSlotArea bölgesini yakalar, 6 sütun x 4 satır parçaya böler,
        /// her parçada puzzle renklerini arar ve UI'daki slot panellerinin rengini günceller.
        /// </summary>
        private void GetSlotColorsButton_Click(object? sender, EventArgs e)
        {
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero || !Win32Native.IsWindow(client.Handle))
                return;

            using (Bitmap? slotAreaBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.PuzzleGameSlotArea))
            {
                if (slotAreaBmp == null) return;

                int cols = 6;
                int rows = 4;
                double colWidth = (double)slotAreaBmp.Width / cols;
                double rowHeight = (double)slotAreaBmp.Height / rows;

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        int slotIndex = (r * cols) + c;
                        if (slotIndex >= _slotPanels.Length) break;

                        int cellX = (int)Math.Round(c * colWidth);
                        int cellY = (int)Math.Round(r * rowHeight);
                        int cellW = (int)Math.Round((c + 1) * colWidth) - cellX;
                        int cellH = (int)Math.Round((r + 1) * rowHeight) - cellY;

                        cellX = Math.Max(0, Math.Min(cellX, slotAreaBmp.Width - 1));
                        cellY = Math.Max(0, Math.Min(cellY, slotAreaBmp.Height - 1));
                        cellW = Math.Max(1, Math.Min(cellW, slotAreaBmp.Width - cellX));
                        cellH = Math.Max(1, Math.Min(cellH, slotAreaBmp.Height - cellY));

                        Color matchedColor = DetectSlotColor(slotAreaBmp, cellX, cellY, cellW, cellH);
                        _slotPanels[slotIndex].BackColor = matchedColor;
                    }
                }

                BotLogger.LogInfo(client.Id, "🧩 [Puzzle] 24 adet puzzle slotunun renkleri başarıyla tarandı ve arayüze aktarıldı.");
            }
        }

        /// <summary>
        /// "Yeni Parça" butonuna tıklandığında:
        /// 1. PuzzleGameChestArea bölgesine tıklar.
        /// 1.1. PuzzleGameSlotArea içerisinde DropItemQuestionYesButton arar ve bulursa tıklar.
        /// 2. Fareyi PuzzleGameSlotArea'nın 10px altına getirir.
        /// 3. Fare imlecinin altındaki 10x10'luk alanda rengi AllPuzzleColors ile eşleştirir.
        /// 4. Seçilen rengi selectedPuzzlePartColor panelinin arkaplanına atar.
        /// </summary>
        private async void SelectNewPuzzlePart_Click(object? sender, EventArgs e)
        {
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero || !Win32Native.IsWindow(client.Handle))
                return;

            try
            {
                selectNewPuzzlePart.Enabled = false;

                // 1. PuzzleGameChestArea'ya tıkla (Sandıktan yeni parça al)
                int chestCenterX = RegionConstants.PuzzleGameChestArea.StartX + (RegionConstants.PuzzleGameChestArea.Width / 2);
                int chestCenterY = RegionConstants.PuzzleGameChestArea.StartY + (RegionConstants.PuzzleGameChestArea.Height / 2);

                BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] Sandığa tıklanıyor ({chestCenterX}, {chestCenterY})...");
                await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, chestCenterX, chestCenterY, fastMove: false);
                await Task.Delay(80);

                // 1.1. PuzzleGameSlotArea içerisinde DropItemQuestionYesButton ara ve bulursan bir kere tıkla
                using (Bitmap? slotAreaBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.PuzzleGameSlotArea))
                {
                    if (slotAreaBmp != null)
                    {
                        var yesMatch = TemplateConstants.Match(slotAreaBmp, TemplateConstants.WindowParts.DropItemQuestionYesButton, threshold: 0.70);
                        if (yesMatch.IsSuccess)
                        {
                            int yesLocalX = RegionConstants.PuzzleGameSlotArea.StartX + yesMatch.Location.X + (yesMatch.Bounds.Width / 2);
                            int yesLocalY = RegionConstants.PuzzleGameSlotArea.StartY + yesMatch.Location.Y + (yesMatch.Bounds.Height / 2);

                            BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] 'DropItemQuestionYesButton' tespit edildi ({yesLocalX}, {yesLocalY}). Tıklanıyor...");
                            await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, yesLocalX, yesLocalY, fastMove: true);
                            await Task.Delay(100);
                        }
                    }
                }

                // 2. Fareyi PuzzleGameSlotArea'nın 10 piksel altına getir
                int targetLocalX = RegionConstants.PuzzleGameSlotArea.StartX + (RegionConstants.PuzzleGameSlotArea.Width / 2);
                int targetLocalY = RegionConstants.PuzzleGameSlotArea.EndY + 10;

                Point targetScreen = HumanMouseService.LocalToScreen(client.Handle, targetLocalX, targetLocalY);
                Win32Native.SetCursorPos(targetScreen.X, targetScreen.Y);
                await Task.Delay(100);

                // 3. Fare konumundaki 10x10'luk alandan ekran görüntüsü al ve puzzle renkleriyle eşleştir
                var sampleRegion = new WindowRegion(targetLocalX - 5, targetLocalY - 5, targetLocalX + 5, targetLocalY + 5);
                using (Bitmap? sampleBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, sampleRegion))
                {
                    if (sampleBmp != null)
                    {
                        Color matchedColor = DetectPieceColor(sampleBmp);

                        // 4. Seçilen rengi selectedPuzzlePartColor arkaplanına ekle
                        selectedPuzzlePartColor.BackColor = matchedColor;

                        BotLogger.LogSuccess(client.Id, $"🧩 [Puzzle] Yeni parça rengi tespit edildi: RGB({matchedColor.R}, {matchedColor.G}, {matchedColor.B})");
                    }
                    else
                    {
                        selectedPuzzlePartColor.BackColor = Color.FromArgb(64, 64, 64);
                    }
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogError(client.Id, $"[Puzzle] Parça seçimi sırasında hata: {ex.Message}");
            }
            finally
            {
                selectNewPuzzlePart.Enabled = true;
            }
        }

        /// <summary>
        /// "Parçayı At" butonuna tıklandığında:
        /// A: PuzzleGameSlotArea içerisinde rastgele bir alana sağ tıklanır.
        /// B: 100ms sonra DropItemQuestionYesButton aranır, bulunursa tıklanır.
        /// C: Fare PuzzleGameSlotArea 10px altında rastgele bir yere gider ve renk kontrol edilir [renk hala aynıysa A'ya döner].
        /// D: selectedPuzzlePartColor rengi tekrar boşa (64, 64, 64) çekilir.
        /// </summary>
        private async void DropPuzzlePart_Click(object? sender, EventArgs e)
        {
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero || !Win32Native.IsWindow(client.Handle))
                return;

            // Eğer yeni parça seçilmemişse (boş renkteyse) işlem yapma
            if (selectedPuzzlePartColor.BackColor == Color.FromArgb(64, 64, 64))
            {
                BotLogger.LogWarning(client.Id, "⚠️ [Puzzle] Atılacak seçili bir puzzle parçası bulunmuyor.");
                return;
            }

            try
            {
                dropPuzzlePart.Enabled = false;
                Color previousColor = selectedPuzzlePartColor.BackColor;
                bool droppedSuccessfully = false;

                for (int attempt = 0; attempt < 5; attempt++)
                {
                    // ADIM A: PuzzleGameSlotArea içerisinde rastgele bir alana sağ tıklanır
                    int randomSlotX = Random.Shared.Next(RegionConstants.PuzzleGameSlotArea.StartX + 10, RegionConstants.PuzzleGameSlotArea.EndX - 10);
                    int randomSlotY = Random.Shared.Next(RegionConstants.PuzzleGameSlotArea.StartY + 10, RegionConstants.PuzzleGameSlotArea.EndY - 10);

                    BotLogger.LogInfo(client.Id, $"🧩 [Puzzle - Adım A] PuzzleGameSlotArea içine sağ tıklanıyor ({randomSlotX}, {randomSlotY})...");
                    await HumanMouseService.Instance.RightClickLocalAsync(client.Handle, randomSlotX, randomSlotY, fastMove: false);

                    // ADIM B: 100ms sonra DropItemQuestionYesButton aranır, bulunursa tıklanır
                    await Task.Delay(100);
                    using (Bitmap? slotBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.PuzzleGameSlotArea))
                    {
                        if (slotBmp != null)
                        {
                            var yesMatch = TemplateConstants.Match(slotBmp, TemplateConstants.WindowParts.DropItemQuestionYesButton, threshold: 0.70);
                            if (yesMatch.IsSuccess)
                            {
                                int yesLocalX = RegionConstants.PuzzleGameSlotArea.StartX + yesMatch.Location.X + (yesMatch.Bounds.Width / 2);
                                int yesLocalY = RegionConstants.PuzzleGameSlotArea.StartY + yesMatch.Location.Y + (yesMatch.Bounds.Height / 2);

                                BotLogger.LogInfo(client.Id, $"🧩 [Puzzle - Adım B] 'DropItemQuestionYesButton' tespit edildi ({yesLocalX}, {yesLocalY}). Tıklanıyor...");
                                await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, yesLocalX, yesLocalY, fastMove: true);
                                await Task.Delay(100);
                            }
                        }
                    }

                    // ADIM C: Fare PuzzleGameSlotArea 10px altında rastgele bir yere gider ve renk kontrol edilir
                    int checkLocalX = Random.Shared.Next(RegionConstants.PuzzleGameSlotArea.StartX + 20, RegionConstants.PuzzleGameSlotArea.EndX - 20);
                    int checkLocalY = RegionConstants.PuzzleGameSlotArea.EndY + 10;

                    Point checkScreen = HumanMouseService.LocalToScreen(client.Handle, checkLocalX, checkLocalY);
                    Win32Native.SetCursorPos(checkScreen.X, checkScreen.Y);
                    await Task.Delay(100);

                    var sampleRegion = new WindowRegion(checkLocalX - 5, checkLocalY - 5, checkLocalX + 5, checkLocalY + 5);
                    using (Bitmap? sampleBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, sampleRegion))
                    {
                        if (sampleBmp != null)
                        {
                            Color detectedColor = DetectPieceColor(sampleBmp);

                            // Eğer renk hala aynıysa (parça atılamadıysa) A adımına geri dön
                            if (detectedColor != Color.FromArgb(64, 64, 64) && detectedColor == previousColor)
                            {
                                BotLogger.LogWarning(client.Id, $"🧩 [Puzzle - Adım C] Parça rengi hala algılanıyor. Tekrar deneniyor (Deneme {attempt + 1})...");
                                continue;
                            }
                        }
                    }

                    droppedSuccessfully = true;
                    break;
                }

                // ADIM D: selectedPuzzlePartColor rengi tekrar boşa çekilir
                selectedPuzzlePartColor.BackColor = Color.FromArgb(64, 64, 64);
                if (droppedSuccessfully)
                {
                    BotLogger.LogSuccess(client.Id, "✅ [Puzzle - Adım D] Puzzle parçası başarıyla yere atıldı ve seçim sıfırlandı.");
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogError(client.Id, $"[Puzzle] Parçayı atma sırasında hata: {ex.Message}");
            }
            finally
            {
                dropPuzzlePart.Enabled = true;
            }
        }

        /// <summary>
        /// 10x10'luk parça görselinde AllPuzzleColors renklerinden en baskın olanı tespit eder.
        /// </summary>
        private static Color DetectPieceColor(Bitmap bmp)
        {
            var colorScores = new Dictionary<Color, int>();
            foreach (var color in Colors.AllPuzzleColors)
            {
                colorScores[color] = 0;
            }

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)bmpData.Scan0;
                    int stride = bmpData.Stride;

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        byte* row = scan0 + (y * stride);
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            byte* px = row + (x * 4);
                            byte b = px[0];
                            byte g = px[1];
                            byte r = px[2];

                            foreach (var target in Colors.AllPuzzleColors)
                            {
                                int diff = Math.Abs(r - target.R) + Math.Abs(g - target.G) + Math.Abs(b - target.B);
                                if (diff <= 35)
                                {
                                    colorScores[target]++;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            var bestMatch = colorScores.OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (bestMatch.Value >= 4)
            {
                return bestMatch.Key;
            }

            return Color.FromArgb(64, 64, 64);
        }

        /// <summary>
        /// Verilen hücre dikdörtgeni içinde AllPuzzleColors renklerinden hangisinin en yüksek yoğunlukta bulunduğunu tespit eder.
        /// </summary>
        private static Color DetectSlotColor(Bitmap bmp, int cellX, int cellY, int cellW, int cellH)
        {
            var colorScores = new Dictionary<Color, int>();
            foreach (var color in Colors.AllPuzzleColors)
            {
                colorScores[color] = 0;
            }

            BitmapData bmpData = bmp.LockBits(
                new Rectangle(cellX, cellY, cellW, cellH),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    byte* scan0 = (byte*)bmpData.Scan0;
                    int stride = bmpData.Stride;

                    for (int y = 0; y < cellH; y++)
                    {
                        byte* row = scan0 + (y * stride);
                        for (int x = 0; x < cellW; x++)
                        {
                            byte* px = row + (x * 4);
                            byte b = px[0];
                            byte g = px[1];
                            byte r = px[2];

                            foreach (var target in Colors.AllPuzzleColors)
                            {
                                int diff = Math.Abs(r - target.R) + Math.Abs(g - target.G) + Math.Abs(b - target.B);
                                if (diff <= 35)
                                {
                                    colorScores[target]++;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            var bestMatch = colorScores.OrderByDescending(kv => kv.Value).FirstOrDefault();
            // En az 8 piksel eşleşmesi varsa tespit edilen rengi ata, yoksa varsayılan boş slot rengi (64, 64, 64) ata
            if (bestMatch.Value >= 8)
            {
                return bestMatch.Key;
            }

            return Color.FromArgb(64, 64, 64);
        }

        #region Puzzle Logs Yönetimi

        private RichTextBox? _rtbLogs;
        private int _logCounter = 0;

        private void InitializeLogPanel()
        {
            _rtbLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 35),
                ForeColor = Color.FromArgb(220, 220, 225),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            puzzleLogs.Controls.Clear();
            puzzleLogs.Padding = new Padding(8);
            puzzleLogs.Controls.Add(_rtbLogs);

            BotLogger.OnLog += BotLogger_OnLog;
        }

        private void BotLogger_OnLog(int clientId, string message, Color color)
        {
            // Sadece Puzzle ile ilgili logları göster
            if (!message.Contains("[Puzzle]", StringComparison.OrdinalIgnoreCase))
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(() => BotLogger_OnLog(clientId, message, color)));
                }
                catch { }
                return;
            }

            if (_rtbLogs == null || _rtbLogs.IsDisposed) return;

            // Her 30 logda bir veya satır sayısı 30'a ulaştığında log penceresini temizle
            _logCounter++;
            if (_logCounter > 30 || _rtbLogs.Lines.Length >= 30)
            {
                _rtbLogs.Clear();
                _logCounter = 1;
            }

            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            string line = $"[{timeStamp}] [Client #{clientId}] {message}\n";

            _rtbLogs.SelectionStart = _rtbLogs.TextLength;
            _rtbLogs.SelectionLength = 0;
            _rtbLogs.SelectionColor = color;
            _rtbLogs.AppendText(line);
            _rtbLogs.SelectionColor = _rtbLogs.ForeColor;

            _rtbLogs.ScrollToCaret();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            BotLogger.OnLog -= BotLogger_OnLog;
            base.OnHandleDestroyed(e);
        }

        #endregion
    }
}
