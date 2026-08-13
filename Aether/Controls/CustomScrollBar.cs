using Aether.Constants;
using Aether.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Aether.Controls
{
    /// <summary>
    /// Colors sınıfındaki renkleri kullanan özel tasarlanmış modern dikey ScrollBar bileşeni.
    /// </summary>
    public class CustomScrollBar : UserControl
    {
        private ScrollableControl? _targetControl;
        private int _minimum = 0;
        private int _maximum = 100;
        private int _value = 0;
        private int _largeChange = 10;

        private bool _isHovered = false;
        private bool _isMouseDown = false;
        private Point _dragStartPoint;
        private int _dragStartValue;

        private CustomScrollMessageFilter? _messageFilter;

        public new event EventHandler? Scroll;
        public event EventHandler? ValueChanged;

        public CustomScrollBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Width = 14;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode && _messageFilter == null)
            {
                _messageFilter = new CustomScrollMessageFilter(this);
                Application.AddMessageFilter(_messageFilter);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_messageFilter != null)
            {
                Application.RemoveMessageFilter(_messageFilter);
                _messageFilter = null;
            }
            base.OnHandleDestroyed(e);
        }

        /// <summary>
        /// Kaydırılacak hedef FlowLayoutPanel veya Panel.
        /// </summary>
        [Category("Behavior")]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public ScrollableControl? TargetControl
        {
            get => _targetControl;
            set
            {
                if (_targetControl != value)
                {
                    if (_targetControl != null)
                    {
                        _targetControl.Resize -= TargetControl_LayoutChanged;
                        _targetControl.ControlAdded -= TargetControl_ControlAddedRemoved;
                        _targetControl.ControlRemoved -= TargetControl_ControlAddedRemoved;

                        foreach (Control child in _targetControl.Controls)
                        {
                            UnhookChildEvents(child);
                        }
                    }

                    _targetControl = value;

                    if (_targetControl != null)
                    {
                        _targetControl.Resize += TargetControl_LayoutChanged;
                        _targetControl.ControlAdded += TargetControl_ControlAddedRemoved;
                        _targetControl.ControlRemoved += TargetControl_ControlAddedRemoved;

                        foreach (Control child in _targetControl.Controls)
                        {
                            HookChildEvents(child);
                        }

                        _targetControl.AutoScroll = true;
                        HideNativeScrollBar();
                    }

                    SyncWithTarget();
                }
            }
        }

        public void HideNativeScrollBar()
        {
            if (_targetControl != null && _targetControl.IsHandleCreated)
            {
                Win32Native.ShowScrollBar(_targetControl.Handle, Win32Native.SB_VERT, false);
                Win32Native.ShowScrollBar(_targetControl.Handle, Win32Native.SB_HORZ, false);
            }
        }

        private void HookChildEvents(Control child)
        {
            if (child == null) return;
            child.SizeChanged -= ChildControl_SizeChanged;
            child.SizeChanged += ChildControl_SizeChanged;
            child.Resize -= ChildControl_SizeChanged;
            child.Resize += ChildControl_SizeChanged;
        }

        private void UnhookChildEvents(Control child)
        {
            if (child == null) return;
            child.SizeChanged -= ChildControl_SizeChanged;
            child.Resize -= ChildControl_SizeChanged;
        }

        private void ChildControl_SizeChanged(object? sender, EventArgs e)
        {
            SyncWithTarget();
        }

        [Category("Behavior")]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                Invalidate();
            }
        }

        [Category("Behavior")]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = Math.Max(_minimum, value);
                Invalidate();
            }
        }

        [Category("Behavior")]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int Value
        {
            get => _value;
            set
            {
                int val = Math.Clamp(value, _minimum, _maximum);
                if (_value != val)
                {
                    _value = val;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                    ApplyScrollToTarget();
                    Invalidate();
                }
            }
        }

        [Category("Behavior")]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int LargeChange
        {
            get => _largeChange;
            set
            {
                _largeChange = Math.Max(1, value);
                Invalidate();
            }
        }

        /// <summary>
        /// Hedef kontrolün boyutları ve içeriğine göre scrollbar aralığını günceller.
        /// </summary>
        public void SyncWithTarget()
        {
            if (_targetControl == null) return;

            HideNativeScrollBar();

            int contentHeight = 0;
            int currentScrollY = Math.Abs(_targetControl.AutoScrollPosition.Y);

            foreach (Control ctrl in _targetControl.Controls)
            {
                if (ctrl.Visible)
                {
                    int absoluteBottom = ctrl.Bottom + currentScrollY + ctrl.Margin.Bottom;
                    contentHeight = Math.Max(contentHeight, absoluteBottom);
                }
            }

            int displayHeight = _targetControl.DisplayRectangle.Height;
            contentHeight = Math.Max(contentHeight, displayHeight);

            int visibleHeight = _targetControl.ClientSize.Height;
            int maxScroll = Math.Max(0, contentHeight - visibleHeight);

            _maximum = maxScroll;
            _largeChange = Math.Max(1, visibleHeight);
            _value = Math.Clamp(currentScrollY, 0, _maximum);

            Visible = maxScroll > 0;
            Invalidate();
        }

        private void TargetControl_LayoutChanged(object? sender, EventArgs e)
        {
            SyncWithTarget();
        }

        private void TargetControl_ControlAddedRemoved(object? sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                HookChildEvents(e.Control);
            }
            SyncWithTarget();
        }

        private void ApplyScrollToTarget()
        {
            if (_targetControl == null) return;

            _targetControl.AutoScrollPosition = new Point(0, _value);
            HideNativeScrollBar();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Rectangle thumbRect = GetThumbRectangle();
                if (thumbRect.Contains(e.Location))
                {
                    _isMouseDown = true;
                    _dragStartPoint = e.Location;
                    _dragStartValue = _value;
                }
                else
                {
                    if (e.Y < thumbRect.Top)
                    {
                        Value -= _largeChange;
                    }
                    else if (e.Y > thumbRect.Bottom)
                    {
                        Value += _largeChange;
                    }
                }
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isMouseDown = false;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isMouseDown)
            {
                int trackHeight = Height - GetThumbHeight();
                if (trackHeight > 0)
                {
                    int deltaY = e.Y - _dragStartPoint.Y;
                    float percentDelta = (float)deltaY / trackHeight;
                    int valueDelta = (int)(percentDelta * (_maximum - _minimum));
                    Value = _dragStartValue + valueDelta;
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int GetThumbHeight()
        {
            if (_maximum <= _minimum) return Height;
            float ratio = (float)_largeChange / (_maximum - _minimum + _largeChange);
            int thumbHeight = (int)(Height * ratio);
            return Math.Max(25, thumbHeight);
        }

        private Rectangle GetThumbRectangle()
        {
            int thumbHeight = GetThumbHeight();
            int trackHeight = Height - thumbHeight;

            int thumbY = 0;
            if (_maximum > _minimum && trackHeight > 0)
            {
                float percent = (float)(_value - _minimum) / (_maximum - _minimum);
                thumbY = (int)(percent * trackHeight);
            }

            return new Rectangle(2, thumbY + 2, Width - 4, thumbHeight - 4);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Arka plan kanalı (Track)
            using (SolidBrush trackBrush = new SolidBrush(Color.FromArgb(30, 30, 35)))
            {
                using (GraphicsPath trackPath = GetRoundedPath(new Rectangle(1, 0, Width - 2, Height), 4))
                {
                    g.FillPath(trackBrush, trackPath);
                }
            }

            if (_maximum <= _minimum) return;

            // Colors sabit sınıfından renk seçimi
            Color thumbColor;
            if (_isMouseDown)
            {
                thumbColor = Colors.YesilKoyu; // Sürükleme esnasında
            }
            else if (_isHovered)
            {
                thumbColor = Colors.MaviAcik;  // Mouse üzerindeyken
            }
            else
            {
                thumbColor = Colors.MaviKoyu;  // Normal durum
            }

            Rectangle thumbRect = GetThumbRectangle();
            if (thumbRect.Width > 0 && thumbRect.Height > 0)
            {
                using (SolidBrush thumbBrush = new SolidBrush(thumbColor))
                {
                    using (GraphicsPath thumbPath = GetRoundedPath(thumbRect, Math.Min(thumbRect.Width, thumbRect.Height) / 2))
                    {
                        g.FillPath(thumbBrush, thumbPath);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;

            int diameter = Math.Max(1, cornerRadius * 2);
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);

            // Sol üst
            path.AddArc(arc, 180, 90);
            // Sağ üst
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // Sağ alt
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // Sol alt
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private class CustomScrollMessageFilter : IMessageFilter
        {
            private readonly CustomScrollBar _scrollBar;

            public CustomScrollMessageFilter(CustomScrollBar scrollBar)
            {
                _scrollBar = scrollBar;
            }

            public bool PreFilterMessage(ref Message m)
            {
                const int WM_MOUSEWHEEL = 0x020A;
                if (m.Msg == WM_MOUSEWHEEL)
                {
                    if (_scrollBar.TargetControl != null && _scrollBar.TargetControl.Visible && _scrollBar.Visible)
                    {
                        Point mousePt = Control.MousePosition;
                        Control target = _scrollBar.TargetControl;

                        if (target.IsHandleCreated)
                        {
                            Rectangle screenBounds = target.RectangleToScreen(target.ClientRectangle);
                            if (screenBounds.Contains(mousePt))
                            {
                                short delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                                int scrollAmount = delta > 0 ? -60 : 60;
                                _scrollBar.Value += scrollAmount;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
