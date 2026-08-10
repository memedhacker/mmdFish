using Aether.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Aether.Controls
{
    /// <summary>
    /// Titreme (Flicker) ve görüntü bozulmalarını engelleyen çift tamponlamalı (DoubleBuffered) FlowLayoutPanel.
    /// Yerel Windows ScrollBar'larını Win32Native üzerinden WndProc seviyesinde pürüzsüzce gizler.
    /// </summary>
    public class DoubleBufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public DoubleBufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideNativeScrollBars();
        }

        public void HideNativeScrollBars()
        {
            if (IsHandleCreated)
            {
                Win32Native.ShowScrollBar(Handle, Win32Native.SB_VERT, false);
                Win32Native.ShowScrollBar(Handle, Win32Native.SB_HORZ, false);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Win32Native.WM_NCCALCSIZE || m.Msg == Win32Native.WM_NCPAINT)
            {
                HideNativeScrollBars();
            }

            base.WndProc(ref m);
        }
    }
}
