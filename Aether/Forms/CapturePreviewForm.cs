using Aether.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Aether.Forms
{
    /// <summary>
    /// Pencerenin tam iç alanını gösteren, fare ile bölge seçimi yaptıran,
    /// belirlenen milisaniyede bir otomatik ekran görüntüsü alıp canlı Template Matching testleri yürüten
    /// ve sonuçları gerçek zamanlı log kutusunda listeleyen gelişmiş test formu.
    /// </summary>
    public class CapturePreviewForm : Form
    {
        private Bitmap _fullImage;
        private readonly IntPtr _sourceHwnd;
        private readonly string _clientName;

        // Seçim Durumları
        private bool _isSelecting = false;
        private Point _dragStartPoint = Point.Empty;
        private Rectangle _currentSelection = Rectangle.Empty;

        // Canlı Eşleşme Tespiti (Overlay Çizimi İçin)
        private readonly List<TemplateMatchResult> _lastLiveMatches = new();
        private readonly object _matchLock = new();

        // Otomatik Yenileme / Test Timer
        private readonly System.Windows.Forms.Timer _autoTestTimer = new();
        private bool _isAutoTesting = false;
        private readonly Stopwatch _perfStopwatch = new();

        // Arayüz Elemanları
        private PictureBox _picBox = null!;
        private Label _lblInfo = null!;
        private Label _lblMousePos = null!;
        private Label _lblCodeSnippet = null!;

        private TextBox _txtStartX = null!;
        private TextBox _txtStartY = null!;
        private TextBox _txtEndX = null!;
        private TextBox _txtEndY = null!;

        private Button _btnAutoTestToggle = null!;
        private NumericUpDown _numInterval = null!;
        private ComboBox _cmbCategory = null!;
        private NumericUpDown _numThreshold = null!;
        private ComboBox _cmbSearchScope = null!;
        private Button _btnManualTest = null!;
        private Button _btnClearLogs = null!;
        private RichTextBox _rtbLogs = null!;

        private Button _btnCopyCode = null!;
        private Button _btnApplyCoords = null!;
        private Button _btnSave = null!;
        private Button _btnCopyImage = null!;
        private Panel _imageContainer = null!;

        public CapturePreviewForm(
            Bitmap fullWindowImage,
            string title = "Pencere Seçim & Canlı Template Matching Test Aracı",
            IntPtr sourceHwnd = default,
            Rectangle initialSelection = default,
            string clientName = "")
        {
            _fullImage = (Bitmap)fullWindowImage.Clone();
            _sourceHwnd = sourceHwnd;
            _clientName = string.IsNullOrWhiteSpace(clientName) ? "Bilinmeyen İstemci" : clientName;

            if (!initialSelection.IsEmpty)
            {
                _currentSelection = initialSelection;
            }

            InitializeCustomUI(title);

            // Timer Yapılandırması
            _autoTestTimer.Interval = (int)_numInterval.Value;
            _autoTestTimer.Tick += AutoTestTimer_Tick;
        }

        private void InitializeCustomUI(string title)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Colors.ArkaPlanKoyu;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            DoubleBuffered = true;
            MinimumSize = new Size(950, 680);

            // Pencere Boyutlandırma
            int targetWidth = Math.Clamp(_fullImage.Width + 100, 980, 1300);
            int targetHeight = Math.Clamp(_fullImage.Height + 360, 720, 1000);
            Size = new Size(targetWidth, targetHeight);

            // 1. ÜST BİLGİ PANELİ
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Colors.ArkaPlanAcik,
                Padding = new Padding(12, 6, 12, 4)
            };

            _lblInfo = new Label
            {
                Text = $"🎮 İstemci: {_clientName} (HWND: 0x{_sourceHwnd.ToInt64():X}) | İç Boyut: {_fullImage.Width}x{_fullImage.Height} px",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Colors.MaviAcik,
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblMousePos = new Label
            {
                Text = "Fare: (X: 0, Y: 0) | [Fare ile sürükleyerek alan seçebilirsiniz]",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 200, 200),
                Dock = DockStyle.Right,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight
            };

            headerPanel.Controls.Add(_lblInfo);
            headerPanel.Controls.Add(_lblMousePos);

            // 2. ÇİFT KATMANLI KONTROL PANELİ (TOOLBAR)
            Panel controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.FromArgb(24, 24, 29),
                Padding = new Padding(10, 4, 10, 4)
            };

            // 2.1 Üst Satır: Koordinat Seçim Kutuları ve Kod Kopyalama
            FlowLayoutPanel topCoordFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            int initStartX = _currentSelection.IsEmpty ? 100 : _currentSelection.Left;
            int initStartY = _currentSelection.IsEmpty ? 100 : _currentSelection.Top;
            int initEndX = _currentSelection.IsEmpty ? Math.Min(_fullImage.Width, 450) : _currentSelection.Right;
            int initEndY = _currentSelection.IsEmpty ? Math.Min(_fullImage.Height, 450) : _currentSelection.Bottom;

            _txtStartX = CreateCoordTextBox(initStartX.ToString());
            _txtStartY = CreateCoordTextBox(initStartY.ToString());
            _txtEndX = CreateCoordTextBox(initEndX.ToString());
            _txtEndY = CreateCoordTextBox(initEndY.ToString());

            _btnApplyCoords = CreateStyledButton("📐 Çiz", Colors.ArkaPlanAcik, Color.White);
            _btnApplyCoords.Click += (s, e) => SyncCoordsFromTextBoxes();

            _btnCopyCode = CreateStyledButton("📋 Kodu Kopyala", Colors.YesilKoyu, Color.White);
            _btnCopyCode.Click += BtnCopyCode_Click;

            _lblCodeSnippet = new Label
            {
                Text = $"CaptureRegion({initStartX}, {initStartY}, {initEndX}, {initEndY});",
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                ForeColor = Colors.YesilAcik,
                AutoSize = true,
                Margin = new Padding(10, 7, 6, 0)
            };

            topCoordFlow.Controls.Add(CreateCoordLabel("Başlangıç X:"));
            topCoordFlow.Controls.Add(_txtStartX);
            topCoordFlow.Controls.Add(CreateCoordLabel("Başlangıç Y:"));
            topCoordFlow.Controls.Add(_txtStartY);
            topCoordFlow.Controls.Add(CreateCoordLabel("Bitiş X:"));
            topCoordFlow.Controls.Add(_txtEndX);
            topCoordFlow.Controls.Add(CreateCoordLabel("Bitiş Y:"));
            topCoordFlow.Controls.Add(_txtEndY);
            topCoordFlow.Controls.Add(_btnApplyCoords);
            topCoordFlow.Controls.Add(_btnCopyCode);
            topCoordFlow.Controls.Add(_lblCodeSnippet);

            // 2.2 Alt Satır: Otomatik Yenileme ve Canlı Template Matching Ayarları
            FlowLayoutPanel bottomTestFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            _btnAutoTestToggle = CreateStyledButton("▶️ Canlı Testi Başlat", Colors.YesilKoyu, Color.White);
            _btnAutoTestToggle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnAutoTestToggle.Click += BtnAutoTestToggle_Click;

            _numInterval = new NumericUpDown
            {
                Minimum = 30,
                Maximum = 5000,
                Value = 250,
                Increment = 50,
                Width = 65,
                Height = 28,
                BackColor = Colors.ArkaPlanKoyu,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 3, 6, 0)
            };
            _numInterval.ValueChanged += (s, e) => _autoTestTimer.Interval = (int)_numInterval.Value;

            _cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 145,
                Height = 28,
                BackColor = Colors.ArkaPlanKoyu,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Margin = new Padding(0, 3, 6, 0)
            };
            _cmbCategory.Items.AddRange(new object[] { "🐟 FishNames (40 Balık)", "📍 Waypoints (6 Şablon)", "🛡️ AutoPass (2 Şablon)", "⭐ Tümü (48 Şablon)" });
            _cmbCategory.SelectedIndex = 0;

            _numThreshold = new NumericUpDown
            {
                Minimum = 50,
                Maximum = 99,
                Value = 80,
                Increment = 2,
                Width = 50,
                Height = 28,
                BackColor = Colors.ArkaPlanKoyu,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(0, 3, 6, 0)
            };

            _cmbSearchScope = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 125,
                Height = 28,
                BackColor = Colors.ArkaPlanKoyu,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                Margin = new Padding(0, 3, 6, 0)
            };
            _cmbSearchScope.Items.AddRange(new object[] { "📐 Seçili Bölgede Ara", "🖼️ Tüm Pencerede Ara" });
            _cmbSearchScope.SelectedIndex = 0;

            _btnManualTest = CreateStyledButton("🔍 Tek Sefer Test", Colors.MaviKoyu, Color.White);
            _btnManualTest.Click += (s, e) => ExecuteSingleTemplateMatchTest();

            _btnClearLogs = CreateStyledButton("🧹 Temizle", Colors.ArkaPlanAcik, Color.White);
            _btnClearLogs.Click += (s, e) => _rtbLogs.Clear();

            bottomTestFlow.Controls.Add(_btnAutoTestToggle);
            bottomTestFlow.Controls.Add(CreateCoordLabel("Aralık:"));
            bottomTestFlow.Controls.Add(_numInterval);
            bottomTestFlow.Controls.Add(CreateCoordLabel("ms | Şablon:"));
            bottomTestFlow.Controls.Add(_cmbCategory);
            bottomTestFlow.Controls.Add(CreateCoordLabel("Eşik: %"));
            bottomTestFlow.Controls.Add(_numThreshold);
            bottomTestFlow.Controls.Add(CreateCoordLabel("Kapsam:"));
            bottomTestFlow.Controls.Add(_cmbSearchScope);
            bottomTestFlow.Controls.Add(_btnManualTest);
            bottomTestFlow.Controls.Add(_btnClearLogs);

            controlPanel.Controls.Add(bottomTestFlow);
            controlPanel.Controls.Add(topCoordFlow);

            // 3. ALT LOG KONSOLU PANELİ (DOCK BOTTOM)
            Panel logPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(18, 18, 22),
                Padding = new Padding(8, 4, 8, 6)
            };

            Panel logHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24,
                BackColor = Colors.ArkaPlanAcik,
                Padding = new Padding(8, 2, 8, 2)
            };

            Label lblLogTitle = new Label
            {
                Text = "📊 CANLI TEMPLATE MATCHING LOG VE EŞLEŞME SONUÇLARI",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Colors.MaviAcik,
                Dock = DockStyle.Left,
                AutoSize = true
            };

            _btnSave = CreateStyledButton("💾 Kaydet (PNG)", Colors.ArkaPlanKoyu, Color.White);
            _btnSave.Height = 22;
            _btnSave.Padding = new Padding(6, 0, 6, 0);
            _btnSave.Click += BtnSave_Click;

            _btnCopyImage = CreateStyledButton("🖼️ Resmi Kopyala", Colors.ArkaPlanKoyu, Color.White);
            _btnCopyImage.Height = 22;
            _btnCopyImage.Padding = new Padding(6, 0, 6, 0);
            _btnCopyImage.Click += BtnCopyImage_Click;

            FlowLayoutPanel logActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            logActions.Controls.Add(_btnSave);
            logActions.Controls.Add(_btnCopyImage);

            logHeader.Controls.Add(lblLogTitle);
            logHeader.Controls.Add(logActions);

            _rtbLogs = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(14, 14, 17),
                ForeColor = Color.Gainsboro,
                Font = new Font("Consolas", 9f, FontStyle.Regular),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            logPanel.Controls.Add(_rtbLogs);
            logPanel.Controls.Add(logHeader);

            // 4. ORTA GÖRSEL ALANI (DOCK FILL)
            _imageContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Colors.ArkaPlanKoyu,
                Padding = new Padding(8)
            };

            _picBox = new PictureBox
            {
                Image = _fullImage,
                SizeMode = PictureBoxSizeMode.AutoSize,
                Location = new Point(8, 8),
                Cursor = Cursors.Cross,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Fare Olayları
            _picBox.MouseDown += PicBox_MouseDown;
            _picBox.MouseMove += PicBox_MouseMove;
            _picBox.MouseUp += PicBox_MouseUp;
            _picBox.Paint += PicBox_Paint;

            _imageContainer.Controls.Add(_picBox);

            // Form Kontrol Sıralaması
            Controls.Add(_imageContainer);
            Controls.Add(controlPanel);
            Controls.Add(headerPanel);
            Controls.Add(logPanel);

            // Başlangıç Seçim Kutusunu Senkronize Et
            if (_currentSelection.IsEmpty)
            {
                _currentSelection = new Rectangle(initStartX, initStartY, initEndX - initStartX, initEndY - initStartY);
            }
            UpdateLabelsAndBoxes(_currentSelection.Left, _currentSelection.Top, _currentSelection.Right, _currentSelection.Bottom);

            AppendLog("✅ Test penceresi hazırlandı. '▶️ Canlı Testi Başlat' butonuna basarak sürekli eşleşme testini çalıştırabilirsiniz.", Color.LightGreen);
        }

        #region Canlı Otomatik Test & Template Matching Döngüsü

        private void BtnAutoTestToggle_Click(object? sender, EventArgs e)
        {
            if (_sourceHwnd == IntPtr.Zero)
            {
                MessageBox.Show("Geçerli bir oyun penceresi (HWND) bağlı değil.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isAutoTesting = !_isAutoTesting;

            if (_isAutoTesting)
            {
                _autoTestTimer.Interval = (int)_numInterval.Value;
                _autoTestTimer.Start();
                _btnAutoTestToggle.Text = "⏸️ Canlı Testi Durdur";
                _btnAutoTestToggle.BackColor = Colors.PembeKoyu;
                AppendLog($"🚀 Canlı otomatik eşleşme testi başlatıldı. (Aralık: {_autoTestTimer.Interval} ms, Eşik: %{_numThreshold.Value})", Colors.MaviAcik);
            }
            else
            {
                _autoTestTimer.Stop();
                _btnAutoTestToggle.Text = "▶️ Canlı Testi Başlat";
                _btnAutoTestToggle.BackColor = Colors.YesilKoyu;

                lock (_matchLock)
                {
                    _lastLiveMatches.Clear();
                }
                _picBox.Invalidate();
                AppendLog("⏹️ Canlı test durduruldu.", Color.Orange);
            }
        }

        private void AutoTestTimer_Tick(object? sender, EventArgs e)
        {
            ExecuteSingleTemplateMatchTest();
        }

        private void ExecuteSingleTemplateMatchTest()
        {
            if (_sourceHwnd == IntPtr.Zero) return;

            try
            {
                _perfStopwatch.Restart();

                // 1. Ekran görüntüsünü arka planda al
                Bitmap? latestBmp = Helpers.WindowCaptureHelper.CaptureWindow(_sourceHwnd);
                if (latestBmp == null)
                {
                    AppendLog("⚠️ Ekran görüntüsü alınamadı (Pencere simge durumunda veya geçersiz).", Color.Orange);
                    return;
                }

                // Görseli güncelle
                _picBox.Image = null;
                _fullImage.Dispose();
                _fullImage = latestBmp;
                _picBox.Image = _fullImage;

                // 2. Arama yapılacak hedef şablon listesini belirle
                IReadOnlyList<string> candidateTemplates = _cmbCategory.SelectedIndex switch
                {
                    0 => TemplateConstants.FishNames.All,
                    1 => TemplateConstants.Waypoints.All,
                    2 => TemplateConstants.AutoPass.All,
                    _ => TemplateConstants.AllTemplates
                };

                double threshold = (double)_numThreshold.Value / 100.0;
                bool searchInSelection = _cmbSearchScope.SelectedIndex == 0 && _currentSelection.Width > 5 && _currentSelection.Height > 5;

                Bitmap searchTarget = _fullImage;
                Bitmap? croppedTarget = null;
                Point searchOffset = Point.Empty;

                if (searchInSelection)
                {
                    int x = Math.Clamp(_currentSelection.X, 0, _fullImage.Width - 1);
                    int y = Math.Clamp(_currentSelection.Y, 0, _fullImage.Height - 1);
                    int w = Math.Min(_currentSelection.Width, _fullImage.Width - x);
                    int h = Math.Min(_currentSelection.Height, _fullImage.Height - y);

                    if (w > 5 && h > 5)
                    {
                        Rectangle cropRect = new Rectangle(x, y, w, h);
                        croppedTarget = _fullImage.Clone(cropRect, PixelFormat.Format32bppArgb);
                        searchTarget = croppedTarget;
                        searchOffset = new Point(x, y);
                    }
                }

                // 3. Template Matching çalıştır
                var bestMatch = TemplateConstants.FindBestMatch(searchTarget, candidateTemplates, minThreshold: threshold);
                _perfStopwatch.Stop();
                long elapsedMs = _perfStopwatch.ElapsedMilliseconds;

                croppedTarget?.Dispose();

                lock (_matchLock)
                {
                    _lastLiveMatches.Clear();

                    if (bestMatch != null && bestMatch.IsSuccess)
                    {
                        // Koordinatları tam pencereye göre ayarla
                        var adjustedMatch = new TemplateMatchResult
                        {
                            IsSuccess = true,
                            TemplatePath = bestMatch.TemplatePath,
                            TemplateName = bestMatch.TemplateName,
                            Confidence = bestMatch.Confidence,
                            Location = new Point(bestMatch.Location.X + searchOffset.X, bestMatch.Location.Y + searchOffset.Y),
                            Bounds = new Rectangle(bestMatch.Location.X + searchOffset.X, bestMatch.Location.Y + searchOffset.Y, bestMatch.Bounds.Width, bestMatch.Bounds.Height)
                        };

                        _lastLiveMatches.Add(adjustedMatch);

                        AppendLog($"[{DateTime.Now:HH:mm:ss.fff}] 🎯 EŞLEŞTİ! [{adjustedMatch.TemplateName}] → Benzerlik: %{adjustedMatch.Confidence * 100:F1} | Konum: ({adjustedMatch.Location.X}, {adjustedMatch.Location.Y}) | Süre: {elapsedMs}ms", Color.LimeGreen);
                    }
                    else
                    {
                        AppendLog($"[{DateTime.Now:HH:mm:ss.fff}] ⏳ Eşleşme yok (En yüksek benzerlik: %{(bestMatch?.Confidence ?? 0) * 100:F1}) | Süre: {elapsedMs}ms", Color.LightGray);
                    }
                }

                _picBox.Invalidate();
            }
            catch (Exception ex)
            {
                AppendLog($"❌ Hata: {ex.Message}", Color.Red);
            }
        }

        private void AppendLog(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendLog(message, color)));
                return;
            }

            _rtbLogs.SelectionStart = _rtbLogs.TextLength;
            _rtbLogs.SelectionLength = 0;
            _rtbLogs.SelectionColor = color;
            _rtbLogs.AppendText(message + Environment.NewLine);
            _rtbLogs.ScrollToCaret();

            // Log boyutu 300 satırı geçerse başından temizle
            if (_rtbLogs.Lines.Length > 300)
            {
                _rtbLogs.Select(0, _rtbLogs.GetFirstCharIndexFromLine(100));
                _rtbLogs.SelectedText = "";
            }
        }

        #endregion

        #region Fare Seçim Olayları ve Çizim

        private void PicBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isSelecting = true;
                _dragStartPoint = ClampPoint(e.Location);
                _currentSelection = new Rectangle(_dragStartPoint, new Size(0, 0));
                _picBox.Invalidate();
            }
        }

        private void PicBox_MouseMove(object? sender, MouseEventArgs e)
        {
            Point pt = ClampPoint(e.Location);
            _lblMousePos.Text = $"Fare: (X: {pt.X}, Y: {pt.Y}) | [Fare ile sürükleyerek alan seçebilirsiniz]";

            if (_isSelecting)
            {
                int x = Math.Min(_dragStartPoint.X, pt.X);
                int y = Math.Min(_dragStartPoint.Y, pt.Y);
                int w = Math.Abs(pt.X - _dragStartPoint.X);
                int h = Math.Abs(pt.Y - _dragStartPoint.Y);

                _currentSelection = new Rectangle(x, y, w, h);
                UpdateLabelsAndBoxes(x, y, x + w, y + h);
                _picBox.Invalidate();
            }
        }

        private void PicBox_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                Point pt = ClampPoint(e.Location);
                int x = Math.Min(_dragStartPoint.X, pt.X);
                int y = Math.Min(_dragStartPoint.Y, pt.Y);
                int w = Math.Abs(pt.X - _dragStartPoint.X);
                int h = Math.Abs(pt.Y - _dragStartPoint.Y);

                if (w > 2 && h > 2)
                {
                    _currentSelection = new Rectangle(x, y, w, h);
                    UpdateLabelsAndBoxes(x, y, x + w, y + h);
                }
                _picBox.Invalidate();
            }
        }

        private void PicBox_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Canlı Template Match Tespiti Varsa Çiz (Yeşil Vurgu Kutusu)
            lock (_matchLock)
            {
                foreach (var match in _lastLiveMatches)
                {
                    if (match.IsSuccess && match.Bounds.Width > 0 && match.Bounds.Height > 0)
                    {
                        using (var matchFill = new SolidBrush(Color.FromArgb(60, 50, 205, 50)))
                        {
                            g.FillRectangle(matchFill, match.Bounds);
                        }
                        using (var matchPen = new Pen(Color.Lime, 3))
                        {
                            g.DrawRectangle(matchPen, match.Bounds);
                        }

                        string matchTag = $"🎯 {match.TemplateName} (%{match.Confidence * 100:F1})";
                        using (Font tagFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                        {
                            SizeF size = g.MeasureString(matchTag, tagFont);
                            int bx = match.Bounds.Left;
                            int by = Math.Max(0, match.Bounds.Top - (int)size.Height - 4);
                            g.FillRectangle(Brushes.DarkGreen, bx, by, size.Width + 6, size.Height + 2);
                            g.DrawString(matchTag, tagFont, Brushes.White, bx + 3, by + 1);
                        }
                    }
                }
            }

            // 2. Kullanıcının Fare ile Seçtiği Alanı Çiz (Mavi Kutu)
            if (_currentSelection.Width > 0 && _currentSelection.Height > 0)
            {
                using (var fillBrush = new SolidBrush(Color.FromArgb(40, 0, 177, 255)))
                {
                    g.FillRectangle(fillBrush, _currentSelection);
                }

                using (var borderPen = new Pen(Colors.MaviKoyu, 2))
                {
                    g.DrawRectangle(borderPen, _currentSelection);
                }

                string tagText = $"X:{_currentSelection.Left}, Y:{_currentSelection.Top} → {_currentSelection.Right}, {_currentSelection.Bottom} ({_currentSelection.Width}x{_currentSelection.Height})";
                using (Font tagFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(tagText, tagFont);
                    int tagX = _currentSelection.Left;
                    int tagY = Math.Max(0, _currentSelection.Top - (int)textSize.Height - 6);

                    Rectangle tagRect = new Rectangle(tagX, tagY, (int)textSize.Width + 8, (int)textSize.Height + 4);
                    using (var tagBgBrush = new SolidBrush(Color.FromArgb(220, 20, 20, 25)))
                    {
                        g.FillRectangle(tagBgBrush, tagRect);
                    }
                    using (var tagBorderPen = new Pen(Colors.MaviKoyu, 1))
                    {
                        g.DrawRectangle(tagBorderPen, tagRect);
                    }
                    g.DrawString(tagText, tagFont, Brushes.White, tagX + 4, tagY + 2);
                }
            }
        }

        private Point ClampPoint(Point p)
        {
            int cx = Math.Clamp(p.X, 0, _fullImage.Width - 1);
            int cy = Math.Clamp(p.Y, 0, _fullImage.Height - 1);
            return new Point(cx, cy);
        }

        #endregion

        #region Senkronizasyon ve Yardımcılar

        private void UpdateLabelsAndBoxes(int startX, int startY, int endX, int endY)
        {
            _txtStartX.Text = startX.ToString();
            _txtStartY.Text = startY.ToString();
            _txtEndX.Text = endX.ToString();
            _txtEndY.Text = endY.ToString();

            int w = Math.Abs(endX - startX);
            int h = Math.Abs(endY - startY);

            _lblCodeSnippet.Text = $"CaptureRegion({startX}, {startY}, {endX}, {endY});  ({w}x{h} px)";
        }

        private void SyncCoordsFromTextBoxes()
        {
            if (int.TryParse(_txtStartX.Text.Trim(), out int sx) &&
                int.TryParse(_txtStartY.Text.Trim(), out int sy) &&
                int.TryParse(_txtEndX.Text.Trim(), out int ex) &&
                int.TryParse(_txtEndY.Text.Trim(), out int ey))
            {
                int minX = Math.Clamp(Math.Min(sx, ex), 0, _fullImage.Width - 1);
                int maxX = Math.Clamp(Math.Max(sx, ex), 0, _fullImage.Width);
                int minY = Math.Clamp(Math.Min(sy, ey), 0, _fullImage.Height - 1);
                int maxY = Math.Clamp(Math.Max(sy, ey), 0, _fullImage.Height);

                _currentSelection = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                UpdateLabelsAndBoxes(minX, minY, maxX, maxY);
                _picBox.Invalidate();
            }
            else
            {
                MessageBox.Show("Lütfen tüm koordinat kutularına geçerli sayılar girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Label CreateCoordLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(6, 6, 2, 0)
            };
        }

        private TextBox CreateCoordTextBox(string defaultValue)
        {
            var txt = new TextBox
            {
                Text = defaultValue,
                Width = 52,
                Height = 26,
                BackColor = Colors.ArkaPlanKoyu,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Margin = new Padding(0, 2, 4, 0)
            };

            txt.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    SyncCoordsFromTextBoxes();
                }
            };

            return txt;
        }

        private Button CreateStyledButton(string text, Color backColor, Color foreColor)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                FlatStyle = FlatStyle.Flat,
                Height = 28,
                AutoSize = true,
                Padding = new Padding(8, 1, 8, 1),
                Margin = new Padding(4, 2, 0, 0),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Colors.CizgiRengi;
            return btn;
        }

        #endregion

        #region Aksiyon Butonları

        private void BtnCopyCode_Click(object? sender, EventArgs e)
        {
            int sx = _currentSelection.Left;
            int sy = _currentSelection.Top;
            int ex = _currentSelection.Right;
            int ey = _currentSelection.Bottom;

            string code = $"Bitmap? bolgeResmi = Helpers.WindowRegionCaptureHelper.CaptureRegion(client.Handle, {sx}, {sy}, {ex}, {ey});";
            Clipboard.SetText(code);

            AppendLog($"📋 C# Kodu panoya kopyalandı: {code}", Color.LightBlue);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"Capture_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string fullPath = Path.Combine(desktop, fileName);

                _fullImage.Save(fullPath, ImageFormat.Png);
                AppendLog($"💾 Görsel Masaüstüne kaydedildi: {fullPath}", Color.LightGreen);
            }
            catch (Exception ex)
            {
                AppendLog($"❌ Kayıt hatası: {ex.Message}", Color.Red);
            }
        }

        private void BtnCopyImage_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_currentSelection.Width > 0 && _currentSelection.Height > 0)
                {
                    using (Bitmap cropped = _fullImage.Clone(_currentSelection, PixelFormat.Format32bppArgb))
                    {
                        Clipboard.SetImage(cropped);
                        AppendLog("🖼️ Seçilen bölge görseli panoya kopyalandı.", Color.LightBlue);
                    }
                }
                else
                {
                    Clipboard.SetImage(_fullImage);
                    AppendLog("🖼️ Tam pencere görseli panoya kopyalandı.", Color.LightBlue);
                }
            }
            catch (Exception ex)
            {
                AppendLog($"❌ Kopyalama hatası: {ex.Message}", Color.Red);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoTestTimer.Stop();
                _autoTestTimer.Dispose();
                _fullImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
