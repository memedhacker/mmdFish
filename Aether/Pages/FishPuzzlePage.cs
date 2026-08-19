using Aether.Constants;
using Aether.Engines;
using Aether.Functions;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aether.Pages
{
    public partial class FishPuzzlePage : BaseBotPage
    {
        private static CancellationTokenSource? _puzzleCts;
        private readonly Panel[] _slotPanels;

        /// <summary>
        /// F8 kısayolu veya acil durdurma tetiklendiğinde çalışan yapboz çözümünü sonlandırır.
        /// </summary>
        public static void CancelSolving()
        {
            if (_puzzleCts != null && !_puzzleCts.IsCancellationRequested)
            {
                try
                {
                    _puzzleCts.Cancel();
                }
                catch { }

                var client = ClientState.Instance.SelectedClient;
                if (client != null)
                {
                    BotLogger.LogWarning(client.Id, "🛑 [Puzzle] F8 tuşuna basıldı! Puzzle çözme döngüsü acil olarak durduruldu.");
                }
            }
        }

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
            puzzleSolveButton.Click += PuzzleSolveButton_Click;
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

                // 1.1. DropItemQuestionArea içerisinde DropItemQuestionYesButton veya OkButton ara ve tıkla
                await DismissConfirmationPopupAsync(client);

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
        /// A: PuzzleGameSlotArea içerisinde sağ tıklanır.
        /// B: DropItemQuestionYesButton veya OkButton taranır ve onaylanır.
        /// C: Fare dışarı alınır ve parça düşüşü doğrulanır.
        /// D: selectedPuzzlePartColor rengi tekrar boşa çekilir.
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
                await DropHeldPieceAsync(client);

                // selectedPuzzlePartColor rengi tekrar boşa çekilir
                selectedPuzzlePartColor.BackColor = Color.FromArgb(64, 64, 64);
                BotLogger.LogSuccess(client.Id, "✅ [Puzzle] Puzzle parçası başarıyla yere atıldı ve seçim sıfırlandı.");
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

        #region Puzzle Çözüm Motoru (Solve Loop)

        /// <summary>
        /// Sandıkta parça kalmadığında:
        /// 1. Ekipman menüsü kontrol edilir ve kapatılır ('I' tuşu döngüsü ve Exit Button).
        /// 2. InventoryPosition içerisinde 'NormalPuzzleChest' şablonu taranır.
        /// 3. Eşleşen sandıklardan rastgele biri seçilerek PuzzleGameChestArea alanına sürüklenip bırakılır.
        /// 4. Başarılı olursa true, sandık bulunamazsa false döner.
        /// </summary>
        private async Task<bool> TryRefillChestFromInventoryAsync(ClientInfo client, CancellationToken ct)
        {
            BotLogger.LogInfo(client.Id, "🧩 [Puzzle] Sandık boşaldı / parça bulunamadı. Ekipman menüsü kontrol ediliyor...");
            await StartupEquipmentMenuFunction.EnsureEquipmentMenuClosedAsync(client, ct);

            if (ct.IsCancellationRequested) return false;

            using (Bitmap? invBmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.InventoryPosition))
            {
                if (invBmp == null)
                {
                    BotLogger.LogWarning(client.Id, "🧩 [Puzzle] Envanter bölgesi taranamadı.");
                    return false;
                }

                var chestMatches = TemplateConstants.MatchAll(invBmp, TemplateConstants.InventoryItems.NormalPuzzleChest, threshold: 0.70);
                if (chestMatches != null && chestMatches.Count > 0)
                {
                    var chosen = chestMatches[Random.Shared.Next(chestMatches.Count)];
                    int fromLocalX = RegionConstants.InventoryPosition.StartX + chosen.Location.X + (chosen.Bounds.Width / 2);
                    int fromLocalY = RegionConstants.InventoryPosition.StartY + chosen.Location.Y + (chosen.Bounds.Height / 2);

                    int chestCenterX = RegionConstants.PuzzleGameChestArea.StartX + (RegionConstants.PuzzleGameChestArea.Width / 2);
                    int chestCenterY = RegionConstants.PuzzleGameChestArea.StartY + (RegionConstants.PuzzleGameChestArea.Height / 2);

                    BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] Envanterde 'NormalPuzzleChest' tespit edildi ({fromLocalX}, {fromLocalY}). Puzzle sandık alanına ({chestCenterX}, {chestCenterY}) sürükleniyor...");

                    await HumanMouseService.Instance.DragAndDropLocalAsync(
                        client.Handle,
                        fromLocalX,
                        fromLocalY,
                        chestCenterX,
                        chestCenterY,
                        fastMove: false,
                        cancellationToken: ct);

                    await Task.Delay(350, ct);

                    // Olası drop veya onay pencerelerini kapat
                    await EnsureDropQuestionDismissedAsync(client);

                    BotLogger.LogSuccess(client.Id, "✅ [Puzzle] Yeni yapboz sandığı başarıyla yerleştirildi! Çözüme devam ediliyor...");
                    return true;
                }
                else
                {
                    BotLogger.LogWarning(client.Id, "⚠️ [Puzzle] Envanterde 'NormalPuzzleChest' sandığı bulunamadı. Çözüm durduruldu.");
                    return false;
                }
            }
        }

        /// <summary>
        /// "▶ Puzzle Çöz" butonuna tıklandığında PredefinedBlueprints tabanlı çözüm motorunu çalıştırır.
        /// Sandıktan parça al → renk tespit → şablon bazlı en iyi pozisyon bul → doluluk kontrolü → yerleştir veya at.
        /// Son parça dahil tüm parçalar yerleştirilir. Her yerleştirmeden sonra DropItemQuestionYesButton ve OkButton aranır.
        /// Yapboz tamamlandığında (24/24), tahtanın sıfırlandığı doğrulanır ve otomatik olarak yeni yapboza baştan başlanır.
        /// Sandık boşaldığında envanteri açıp 'NormalPuzzleChest' sandığını sürükleyerek çözüme devam eder.
        /// F8 tuşu acil durdurma kısayoludur.
        /// </summary>
        private async void PuzzleSolveButton_Click(object? sender, EventArgs e)
        {
            var client = ClientState.Instance.SelectedClient;
            if (client == null || client.Handle == IntPtr.Zero || !Win32Native.IsWindow(client.Handle))
                return;

            puzzleSolveButton.Enabled = false;
            selectNewPuzzlePart.Enabled = false;
            dropPuzzlePart.Enabled = false;

            _puzzleCts = new CancellationTokenSource();
            var ct = _puzzleCts.Token;

            var engine = new FishPuzzleSolveEngine();
            const int maxIterationsPerPuzzle = 50;
            int totalSolvedPuzzles = 0;

            try
            {
                while (!ct.IsCancellationRequested && (Win32Native.GetAsyncKeyState((int)Win32Native.VK_F8) & 0x8000) == 0)
                {
                    // ── Yeni Yapboz Başlangıcı ──
                    Color[] boardColors = ScanBoardColors(client);
                    engine.Reset();
                    engine.UpdateBoard(boardColors);
                    UpdateSlotPanels(boardColors);

                    BotLogger.LogInfo(client.Id, $"🧩 [Puzzle #{totalSolvedPuzzles + 1}] Çözüm başlatılıyor. Tahta: {engine.FilledCount}/24 dolu, Aday şablon sayısı: {engine.ActiveBlueprintCount}. (Durdurmak için F8)");

                    int dropCount = 0;
                    int placeCount = 0;
                    bool puzzleCompleted = false;

                    for (int iter = 0; iter < maxIterationsPerPuzzle; iter++)
                    {
                        if (ct.IsCancellationRequested || (Win32Native.GetAsyncKeyState((int)Win32Native.VK_F8) & 0x8000) != 0)
                        {
                            BotLogger.LogWarning(client.Id, "🛑 [Puzzle] F8 Acil Durdurma tuşuna basıldı! Çözüm sonlandırılıyor.");
                            return;
                        }

                        if (engine.IsBoardFull)
                        {
                            puzzleCompleted = true;
                            break;
                        }

                        // ── 1. Sandıktan yeni parça al ──
                        await ClickChestAsync(client);

                        // ── 1.1. Sandık tıklaması sonrası popup kontrolü ──
                        await EnsureDropQuestionDismissedAsync(client);

                        // ── 2. Parça rengini tespit et (popup olmadığından emin olduktan sonra) ──
                        Color pieceColor = await DetectHeldPieceColorAsync(client);
                        var piece = PuzzleConstants.GetPieceByColor(pieceColor);

                        if (piece == null)
                        {
                            if (pieceColor == Color.FromArgb(64, 64, 64))
                            {
                                BotLogger.LogInfo(client.Id, "🧩 [Puzzle] Sandıktan parça alınamadı (sandık boş). Envanterden yeni yapboz sandığı aranıyor...");
                                bool refilled = await TryRefillChestFromInventoryAsync(client, ct);
                                if (refilled)
                                {
                                    // Yeni sandık yerleştirildi, hamle sayısını harcamadan tekrar sandığa tıkla
                                    iter--;
                                    continue;
                                }
                                else
                                {
                                    return;
                                }
                            }

                            BotLogger.LogWarning(client.Id, $"🧩 [Puzzle] Bilinmeyen parça rengi: RGB({pieceColor.R},{pieceColor.G},{pieceColor.B}). Parça atılıyor.");
                            await DropHeldPieceAsync(client);
                            dropCount++;
                            continue;
                        }

                        // ── 3. PredefinedBlueprints ile en iyi pozisyonu hesapla ──
                        var bestPos = engine.FindBestBlueprintPlacement(piece);

                        if (bestPos == null)
                        {
                            BotLogger.LogWarning(client.Id, $"🧩 [Puzzle] {piece.Name} ({piece.Code}) parçası için aktif şablonlarda uygun pozisyon yok. Parça atılıyor.");
                            await DropHeldPieceAsync(client);
                            dropCount++;
                            continue;
                        }

                        int targetRow = bestPos.Value.Row;
                        int targetCol = bestPos.Value.Col;

                        // ── 4. Yerleşim öncesi doluluk oranı kontrolü ──
                        var (afterFilled, occupancyRate, isLastPiece) = engine.CheckOccupancyAfterPlacement(piece, targetRow, targetCol);
                        BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] Parça yerleşim kontrolü: {piece.Name} ({piece.Code}) → ({targetRow},{targetCol}). Yerleştirme sonrası doluluk: {afterFilled}/24 (%{occupancyRate * 100.0:F1}){(isLastPiece ? " [SON PARÇA]" : "")}.");

                        // ── 5. Parçayı hedef slota yerleştir (Son parça dahil!) ──
                        await PlacePieceAtSlotAsync(client, targetRow, targetCol);
                        placeCount++;

                        // ── 5.1. Yerleştirme sonrası anında popup kontrolü ──
                        await EnsureDropQuestionDismissedAsync(client);

                        // ── 6. Tüm slotların renklerini kesin olarak tara ve Engine / UI güncelle ──
                        engine.PlacePiece(piece, targetRow, targetCol);
                        boardColors = ScanBoardColors(client);
                        engine.UpdateBoard(boardColors);
                        UpdateSlotPanels(boardColors);

                        // ── 6.1. Yerleştirme ve tarama sonrası tekrar popup kontrolü ──
                        await EnsureDropQuestionDismissedAsync(client);

                        BotLogger.LogSuccess(client.Id, $"🧩 [Puzzle] {piece.Name} ({piece.Code}) → ({targetRow},{targetCol}). Dolu: {engine.FilledCount}/24 (Kalan aday şablon: {engine.ActiveBlueprintCount})");

                        // ── 7. 24 slot dolduysa tamamlandı olarak işaretle ──
                        if (engine.IsBoardFull || engine.FilledCount >= PuzzleConstants.TotalSlots || isLastPiece)
                        {
                            puzzleCompleted = true;
                            break;
                        }

                        await Task.Delay(50, ct);
                    }

                    if (puzzleCompleted || engine.IsBoardFull || engine.FilledCount >= PuzzleConstants.TotalSlots)
                    {
                        totalSolvedPuzzles++;
                        BotLogger.LogSuccess(client.Id, $"🎉 [Puzzle #{totalSolvedPuzzles}] 24 slotun tamamı doldu! Puzzle başarıyla tamamlandı. (Yerleştirilen: {placeCount}, Atılan: {dropCount})");

                        // ── 8. Tahtanın sıfırlanmasını bekle ve doğrula ──
                        BotLogger.LogInfo(client.Id, "🧩 [Puzzle] Oyunun tamamlanması ve tahtanın sıfırlanması bekleniyor...");
                        bool isReset = false;

                        for (int check = 0; check < 20; check++) // max ~6-7 saniye bekle
                        {
                            if (ct.IsCancellationRequested || (Win32Native.GetAsyncKeyState((int)Win32Native.VK_F8) & 0x8000) != 0)
                                return;

                            await Task.Delay(300, ct);
                            await EnsureDropQuestionDismissedAsync(client);

                            boardColors = ScanBoardColors(client);
                            UpdateSlotPanels(boardColors);

                            Color emptyColor = Color.FromArgb(64, 64, 64);
                            int filled = boardColors.Count(c => c != emptyColor);

                            if (filled == 0)
                            {
                                isReset = true;
                                break;
                            }
                        }

                        if (isReset)
                        {
                            BotLogger.LogSuccess(client.Id, "✅ [Puzzle] Tahtanın sıfırlandığı doğrulandı. Yeni yapboza başlanıyor!");
                            await Task.Delay(500, ct);
                            continue; // while döngüsünde sonraki yapboza geç
                        }
                        else
                        {
                            BotLogger.LogWarning(client.Id, "🧩 [Puzzle] Tahta sıfırlanma zaman aşımına uğradı. Çözüm durduruldu.");
                            break;
                        }
                    }
                    else
                    {
                        BotLogger.LogWarning(client.Id, $"🧩 [Puzzle] 50 hamle limitine ulaşıldı ancak tahta tamamlanamadı. Çözüm durduruldu. (Yerleştirilen: {placeCount}, Atılan: {dropCount})");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                BotLogger.LogWarning(client.Id, "🛑 [Puzzle] Puzzle çözme işlemi iptal edildi.");
            }
            catch (Exception ex)
            {
                BotLogger.LogError(client.Id, $"[Puzzle] Çözüm sırasında hata: {ex.Message}");
            }
            finally
            {
                puzzleSolveButton.Enabled = true;
                selectNewPuzzlePart.Enabled = true;
                dropPuzzlePart.Enabled = true;
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Yardımcı Metotlar (Solve Loop için)
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Fare imlecini PuzzleGameSlotArea bölgesinin dışına (15px altına) çeker.
        /// Böylece slot taramalarında veya popup algılamalarında imleç pikselleri örtmez.
        /// </summary>
        private static void MoveCursorOutsideSlotArea(ClientInfo client)
        {
            int safeX = RegionConstants.PuzzleGameSlotArea.StartX + (RegionConstants.PuzzleGameSlotArea.Width / 2);
            int safeY = RegionConstants.PuzzleGameSlotArea.EndY + 15;
            Point screenPos = HumanMouseService.LocalToScreen(client.Handle, safeX, safeY);
            Win32Native.SetCursorPos(screenPos.X, screenPos.Y);
        }

        /// <summary> Tahtayı tarar ve 24 slotun renklerini Color dizisi olarak döner. </summary>
        private Color[] ScanBoardColors(ClientInfo client)
        {
            MoveCursorOutsideSlotArea(client);

            Color[] colors = new Color[PuzzleConstants.TotalSlots];
            Color emptyColor = Color.FromArgb(64, 64, 64);
            for (int i = 0; i < colors.Length; i++) colors[i] = emptyColor;

            using (Bitmap? bmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.PuzzleGameSlotArea))
            {
                if (bmp == null) return colors;

                double colWidth = (double)bmp.Width / PuzzleConstants.GridCols;
                double rowHeight = (double)bmp.Height / PuzzleConstants.GridRows;

                for (int r = 0; r < PuzzleConstants.GridRows; r++)
                {
                    for (int c = 0; c < PuzzleConstants.GridCols; c++)
                    {
                        int idx = (r * PuzzleConstants.GridCols) + c;
                        int cellX = (int)Math.Round(c * colWidth);
                        int cellY = (int)Math.Round(r * rowHeight);
                        int cellW = (int)Math.Round((c + 1) * colWidth) - cellX;
                        int cellH = (int)Math.Round((r + 1) * rowHeight) - cellY;

                        cellX = Math.Max(0, Math.Min(cellX, bmp.Width - 1));
                        cellY = Math.Max(0, Math.Min(cellY, bmp.Height - 1));
                        cellW = Math.Max(1, Math.Min(cellW, bmp.Width - cellX));
                        cellH = Math.Max(1, Math.Min(cellH, bmp.Height - cellY));

                        colors[idx] = DetectSlotColor(bmp, cellX, cellY, cellW, cellH);
                    }
                }
            }
            return colors;
        }

        /// <summary> Color dizisini UI slot panellerine yansıtır. </summary>
        private void UpdateSlotPanels(Color[] colors)
        {
            for (int i = 0; i < Math.Min(colors.Length, _slotPanels.Length); i++)
            {
                _slotPanels[i].BackColor = colors[i];
            }
        }

        /// <summary> PuzzleGameChestArea merkezine sol tıklar. </summary>
        private async Task ClickChestAsync(ClientInfo client)
        {
            int cx = RegionConstants.PuzzleGameChestArea.StartX + (RegionConstants.PuzzleGameChestArea.Width / 2);
            int cy = RegionConstants.PuzzleGameChestArea.StartY + (RegionConstants.PuzzleGameChestArea.Height / 2);
            await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, cx, cy, fastMove: false);
            await Task.Delay(80);
        }

        /// <summary>
        /// DropItemQuestionArea içerisinde 'DropItemQuestionYesButton' veya 'OkButton' şablonlarını arar.
        /// Herhangi biri tespit edilirse üzerine tıklar, fareyi dışarı çeker ve double-check ile butonun kapandığını doğrular.
        /// Bir butona tıklanıp kapatıldıysa true, hiçbir popup bulunamazsa false döner.
        /// </summary>
        private async Task<bool> DismissConfirmationPopupAsync(ClientInfo client, int maxRetries = 6)
        {
            MoveCursorOutsideSlotArea(client);
            await Task.Delay(40);

            bool anyClicked = false;

            for (int retry = 0; retry < maxRetries; retry++)
            {
                using (Bitmap? bmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, RegionConstants.DropItemQuestionArea))
                {
                    if (bmp == null) return anyClicked;

                    // 1. DropItemQuestionYesButton kontrolü
                    var yesMatch = TemplateConstants.Match(bmp, TemplateConstants.WindowParts.DropItemQuestionYesButton, threshold: 0.70);
                    if (yesMatch.IsSuccess)
                    {
                        int lx = RegionConstants.DropItemQuestionArea.StartX + yesMatch.Location.X + (yesMatch.Bounds.Width / 2);
                        int ly = RegionConstants.DropItemQuestionArea.StartY + yesMatch.Location.Y + (yesMatch.Bounds.Height / 2);

                        BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] 'DropItemQuestionYesButton' tespit edildi ({lx}, {ly}). Tıklanıyor...");
                        await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, lx, ly, fastMove: true);
                        anyClicked = true;

                        await Task.Delay(60);
                        MoveCursorOutsideSlotArea(client);
                        await Task.Delay(50);
                        continue;
                    }

                    // 2. OkButton kontrolü
                    var okMatch = TemplateConstants.Match(bmp, TemplateConstants.WindowParts.OkButton, threshold: 0.70);
                    if (okMatch.IsSuccess)
                    {
                        int lx = RegionConstants.DropItemQuestionArea.StartX + okMatch.Location.X + (okMatch.Bounds.Width / 2);
                        int ly = RegionConstants.DropItemQuestionArea.StartY + okMatch.Location.Y + (okMatch.Bounds.Height / 2);

                        BotLogger.LogInfo(client.Id, $"🧩 [Puzzle] 'OkButton' tespit edildi ({lx}, {ly}). Tıklanıyor...");
                        await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, lx, ly, fastMove: true);
                        anyClicked = true;

                        await Task.Delay(60);
                        MoveCursorOutsideSlotArea(client);
                        await Task.Delay(50);
                        continue;
                    }

                    // İki buton da yoksa popup kapalıdır
                    break;
                }
            }

            return anyClicked;
        }

        /// <summary>
        /// Fareyi slot alanı dışına çeker ve DropItemQuestionArea içerisinde DropItemQuestionYesButton veya OkButton arar.
        /// Buton bulunursa üzerine tıklar, fareyi tekrar dışarı çeker ve butonun kapandığını teyit etmek için double-check yapar.
        /// </summary>
        private async Task EnsureDropQuestionDismissedAsync(ClientInfo client)
        {
            await DismissConfirmationPopupAsync(client);
        }

        /// <summary>
        /// PuzzleGameSlotArea'nın 10px altında fare konumundaki 10x10 alan ile
        /// elde tutulan parçanın rengini tespit eder.
        /// </summary>
        private async Task<Color> DetectHeldPieceColorAsync(ClientInfo client)
        {
            int targetX = RegionConstants.PuzzleGameSlotArea.StartX + (RegionConstants.PuzzleGameSlotArea.Width / 2);
            int targetY = RegionConstants.PuzzleGameSlotArea.EndY + 10;

            Point screen = HumanMouseService.LocalToScreen(client.Handle, targetX, targetY);
            Win32Native.SetCursorPos(screen.X, screen.Y);
            await Task.Delay(80);

            var sampleRegion = new WindowRegion(targetX - 5, targetY - 5, targetX + 5, targetY + 5);
            using (Bitmap? bmp = WindowRegionCaptureHelper.CaptureRegion(client.Handle, sampleRegion))
            {
                if (bmp != null)
                    return DetectPieceColor(bmp);
            }
            return Color.FromArgb(64, 64, 64);
        }

        /// <summary>
        /// Elde tutulan parçayı atar:
        /// 1. PuzzleGameSlotArea içinde sağ tıklar ve fareyi slot dışına çeker.
        /// 2. DropItemQuestionYesButton veya OkButton görünene kadar bekler.
        /// 3. Göründüğünde tıklar, fareyi dışarı çeker ve double-check ile kapandığını doğrular.
        /// 4. Butonlardan birine tıklanmadan asla bir sonraki adıma geçmez.
        /// </summary>
        private async Task DropHeldPieceAsync(ClientInfo client)
        {
            bool dismissed = false;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                int rx = Random.Shared.Next(RegionConstants.PuzzleGameSlotArea.StartX + 10, RegionConstants.PuzzleGameSlotArea.EndX - 10);
                int ry = Random.Shared.Next(RegionConstants.PuzzleGameSlotArea.StartY + 10, RegionConstants.PuzzleGameSlotArea.EndY - 10);
                await HumanMouseService.Instance.RightClickLocalAsync(client.Handle, rx, ry, fastMove: false);
                await Task.Delay(60);

                // Fareyi hemen slot alanı dışına çek
                MoveCursorOutsideSlotArea(client);
                await Task.Delay(60);

                // DropItemQuestionYesButton veya OkButton bulunana kadar bekle ve tıkla (max 15 deneme x 60ms = ~900ms)
                for (int waitBtn = 0; waitBtn < 15; waitBtn++)
                {
                    if (await DismissConfirmationPopupAsync(client, maxRetries: 3))
                    {
                        dismissed = true;
                        break;
                    }
                    await Task.Delay(60);
                }

                if (dismissed)
                    break;
            }

            MoveCursorOutsideSlotArea(client);
            await Task.Delay(50);
        }

        /// <summary>
        /// Parçayı tahtadaki belirli slota (row, col) yerleştirir.
        /// Slot merkezine sol tıklar ve ardından fareyi hemen slot alanı dışına çeker.
        /// </summary>
        private async Task PlacePieceAtSlotAsync(ClientInfo client, int row, int col)
        {
            double slotWidth = (double)RegionConstants.PuzzleGameSlotArea.Width / PuzzleConstants.GridCols;
            double slotHeight = (double)RegionConstants.PuzzleGameSlotArea.Height / PuzzleConstants.GridRows;

            int clickX = RegionConstants.PuzzleGameSlotArea.StartX + (int)(col * slotWidth + slotWidth / 2);
            int clickY = RegionConstants.PuzzleGameSlotArea.StartY + (int)(row * slotHeight + slotHeight / 2);

            await HumanMouseService.Instance.LeftClickLocalAsync(client.Handle, clickX, clickY, fastMove: false);
            await Task.Delay(80);

            // Fareyi slot alanı dışına çek
            MoveCursorOutsideSlotArea(client);
            await Task.Delay(40);
        }

        #endregion

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
