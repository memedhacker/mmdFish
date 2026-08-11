using Aether.Models;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Aether.Helpers
{
    /// <summary>
    /// Sistemde çalışan 'metin2client' (veya benzeri oyun penceresi) süreçlerini tespit ederek
    /// UI üzerindeki ComboBox'a dolduran yardımcı sınıf.
    /// </summary>
    public static class GameWindowProcessHelper
    {
        private const string TargetProcessName = "metin2client";

        /// <summary>
        /// Sistemdeki oyun penceresi süreçlerini tarar ve GameWindowProcessInfo listesi olarak döner.
        /// </summary>
        public static List<GameWindowProcessInfo> GetActiveGameWindowProcesses()
        {
            var result = new List<GameWindowProcessInfo>();

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
                            result.Add(new GameWindowProcessInfo
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
        /// Aktif oyun penceresi süreçlerini tarar ve verilen UIComboBox içerisine ekler.
        /// Ayrıca kapanmış olan oyun pencerelerinin HWND'lerini ClientState üzerinden otomatik temizler.
        /// İlk sıraya varsayılan olarak '-- Seç --' metnini yerleştirir.
        /// </summary>
        public static void PopulateGameWindowComboBox(UIComboBox comboBox)
        {
            if (comboBox == null) return;

            comboBox.Items.Clear();
            comboBox.Items.Add("-- Seç --");

            var activeWindows = GetActiveGameWindowProcesses();
            var activeHandles = new HashSet<IntPtr>();

            foreach (var windowInfo in activeWindows)
            {
                comboBox.Items.Add(windowInfo);
                activeHandles.Add(windowInfo.Handle);
            }

            // Kapanmış pencerelerin HWND state kayıtlarını temizle
            Aether.States.ClientState.Instance.ValidateAndCleanInvalidHandles(activeHandles);

            comboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// ComboBox ögeleri arasından verilen HWND (handle) değerine sahip ögeyi seçer.
        /// Eğer handle IntPtr.Zero ise veya eşleşen süreç listede yoksa varsayılan 0. indeksi ('-- Seç --') seçer.
        /// </summary>
        public static void SelectMatchingHandleInComboBox(UIComboBox comboBox, IntPtr targetHandle)
        {
            if (comboBox == null || comboBox.Items.Count == 0) return;

            if (targetHandle != IntPtr.Zero)
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is GameWindowProcessInfo procInfo && procInfo.Handle == targetHandle)
                    {
                        comboBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            // Eşleşme yoksa veya handle boşsa 0. indeksi ('-- Seç --') seç
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
