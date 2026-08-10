using System;

namespace Aether.Models
{
    /// <summary>
    /// Çalışan client sürecinin (Process) pencere adı, PID ve HWND (Handle) bilgilerini temsil eden model.
    /// </summary>
    public class ClientProcessInfo
    {
        public int ProcessId { get; set; }
        public IntPtr Handle { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Title)
                ? $"{ProcessName} (PID: {ProcessId} | HWND: 0x{Handle.ToInt64():X})"
                : $"{Title} (PID: {ProcessId} | HWND: 0x{Handle.ToInt64():X})";
        }
    }
}
