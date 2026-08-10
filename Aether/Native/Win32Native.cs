using System;
using System.Runtime.InteropServices;

namespace Aether.Native
{
    /// <summary>
    /// Windows Win32 API fonksiyonları ve sabitlerini barındıran merkezi P/Invoke sınıfı.
    /// </summary>
    public static class Win32Native
    {
        #region Win32 Constants

        // Windows Mesajları (WM)
        public const int WM_HOTKEY = 0x0312;
        public const int WM_NCCALCSIZE = 0x0083;
        public const int WM_NCPAINT = 0x0085;

        // Window Stil Sabitleri
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int WS_EX_LAYERED = 0x80000;
        public const uint WDA_EXCLUDE = 0x00000003;

        // ScrollBar Sabitleri
        public const int SB_HORZ = 0;
        public const int SB_VERT = 1;
        public const int SB_BOTH = 3;

        // Fare Eylem Sabitleri
        public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        public const uint MOUSEEVENTF_LEFTUP = 0x04;

        // Klavye Eylem Sabitleri
        public const uint KEYEVENTF_KEYUP = 0x0002;

        // Sanal Tuş Kodları (Virtual Keys)
        public const uint VK_SPACE = 0x20;
        public const uint VK_ESCAPE = 0x1B;
        public const uint VK_1 = 0x31;
        public const uint VK_2 = 0x32;
        public const uint VK_3 = 0x33;
        public const uint VK_4 = 0x34;
        public const uint VK_5 = 0x35;
        public const uint VK_CONTROL = 0x11;
        public const uint VK_G = 0x47;

        #endregion

        #region User32.dll Functions

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        [DllImport("user32.dll")]
        public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion
    }
}
