using Aether.Models;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Aether.Helpers
{
    /// <summary>
    /// Sistemde çalışan 'metin2client' (veya benzeri metin2) süreçlerini tespit ederek
    /// UI üzerindeki ComboBox'a dolduran yardımcı sınıf.
    /// </summary>
    public static class ClientProcessHelper
    {
        private const string TargetProcessName = "metin2client";

        /// <summary>
        /// Sistemdeki 'metin2client' süreçlerini tarar ve ClientProcessInfo listesi olarak döner.
        /// </summary>
        public static List<ClientProcessInfo> GetActiveClientProcesses()
        {
            var result = new List<ClientProcessInfo>();

            try
            {
                // 'metin2client' adı altındaki tüm süreçleri al
                Process[] processes = Process.GetProcessesByName(TargetProcessName);

                // Eğer metin2client bulunamazsa 'metin2' adındaki süreçleri de ara (yedek)
                if (processes.Length == 0)
                {
                    processes = Process.GetProcessesByName("metin2");
                }

                foreach (var proc in processes)
                {
                    try
                    {
                        if (!proc.HasExited && proc.MainWindowHandle != IntPtr.Zero)
                        {
                            result.Add(new ClientProcessInfo
                            {
                                ProcessId = proc.Id,
                                Handle = proc.MainWindowHandle,
                                Title = proc.MainWindowTitle,
                                ProcessName = proc.ProcessName
                            });
                        }
                    }
                    catch
                    {
                        // Erişim yetkisi olmayan veya kapanmakta olan süreçleri atla
                    }
                }
            }
            catch
            {
                // Genel süreç tarama hatası durumunda boş liste döner
            }

            return result;
        }

        /// <summary>
        /// Aktif 'metin2client' süreçlerini tarar ve verilen UIComboBox içerisine ekler.
        /// İlk sıraya varsayılan olarak '-- Seç --' metnini yerleştirir.
        /// </summary>
        public static void PopulateClientComboBox(UIComboBox comboBox)
        {
            if (comboBox == null) return;

            comboBox.Items.Clear();
            comboBox.Items.Add("-- Seç --");

            var activeClients = GetActiveClientProcesses();

            foreach (var clientInfo in activeClients)
            {
                comboBox.Items.Add(clientInfo);
            }

            comboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Verilen pencere tutacağını (HWND) ekranda en öne getirir ve simge durumundaysa geri yükler.
        /// </summary>
        public static void BringWindowToFront(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                Aether.Native.Win32Native.ShowWindow(hWnd, Aether.Native.Win32Native.SW_RESTORE);
                Aether.Native.Win32Native.SetForegroundWindow(hWnd);
            }
        }
    }
}
