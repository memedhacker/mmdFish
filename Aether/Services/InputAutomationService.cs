using Aether.Native;
using System;
using System.Threading.Tasks;

namespace Aether.Services
{
    /// <summary>
    /// Donanımsal klavye ve fare simülasyon işlemlerinden sorumlu otomasyon servisi.
    /// </summary>
    public class InputAutomationService
    {
        private static readonly Lazy<InputAutomationService> _instance = new Lazy<InputAutomationService>(() => new InputAutomationService());
        public static InputAutomationService Instance => _instance.Value;

        private readonly Random _random = new Random();

        /// <summary>
        /// Donanımsal tuş basma simülasyonu çalıştırır (CTRL+G -> CTRL+G -> 1 -> 2).
        /// </summary>
        public async Task TriggerMacroSequenceAsync(Action<string>? logger = null)
        {
            try
            {
                byte ctrlScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_CONTROL, 0);
                byte gScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_G, 0);
                byte oneScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_1, 0);
                byte twoScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_2, 0);

                // --- 1. KEZ CTRL + G ---
                BotLogger.LogKey(0, "CTRL + G");
                Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, 0, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, 0, 0);
                await Task.Delay(35);
                Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, Win32Native.KEYEVENTF_KEYUP, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, Win32Native.KEYEVENTF_KEYUP, 0);

                await Task.Delay(_random.Next(300, 350));

                // --- 2. KEZ CTRL + G ---
                BotLogger.LogKey(0, "CTRL + G");
                Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, 0, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, 0, 0);
                await Task.Delay(35);
                Win32Native.keybd_event((byte)Win32Native.VK_G, gScan, Win32Native.KEYEVENTF_KEYUP, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_CONTROL, ctrlScan, Win32Native.KEYEVENTF_KEYUP, 0);

                await Task.Delay(_random.Next(150, 200));

                // --- 1 TUŞU ---
                BotLogger.LogKey(0, "1");
                Win32Native.keybd_event((byte)Win32Native.VK_1, oneScan, 0, 0);
                await Task.Delay(35);
                Win32Native.keybd_event((byte)Win32Native.VK_1, oneScan, Win32Native.KEYEVENTF_KEYUP, 0);

                await Task.Delay(_random.Next(150, 200));

                // --- 2 TUŞU ---
                BotLogger.LogKey(0, "2");
                Win32Native.keybd_event((byte)Win32Native.VK_2, twoScan, 0, 0);
                await Task.Delay(35);
                Win32Native.keybd_event((byte)Win32Native.VK_2, twoScan, Win32Native.KEYEVENTF_KEYUP, 0);

                logger?.Invoke("Makro dizisi başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                logger?.Invoke($"Makro Hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Donanımsal sol fare tıklaması tetikler.
        /// </summary>
        public void SendLeftClick()
        {
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Win32Native.mouse_event(Win32Native.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
