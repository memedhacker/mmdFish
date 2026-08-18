using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Native;
using Aether.Services;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Ekipman menüsünün açılıp kapatılmasını yöneten modül.
    /// EquipmentMenuTitle bulunamazsa 'I' tuşuna basarak arar ve açıldığında ExitButton'a tıklayarak menüyü kapatır.
    /// </summary>
    public static class StartupEquipmentMenuFunction
    {
        /// <summary>
        /// EquipmentMenuTitlePosition alanında EquipmentMenuTitle şablonunu arar.
        /// Bulunamazsa 'I' tuşuna basarak (100ms aralıkla) menü açılana kadar tarar.
        /// Menü açıldığında EquipmentMenuExitButtonPosition alanındaki kapatma butonunu tespit eder,
        /// Windows faresini insansı kavisle (Bézier) butonun üzerine götürüp tıklar ve menüyü kapatır.
        /// </summary>
        public static async Task EnsureEquipmentMenuClosedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            Debug.WriteLine($"[StartupEquipmentMenu] Client #{clientInfo.Id} -> Ekipman menüsü kontrol ediliyor...");

            bool titleFound = false;
            int maxAttempts = 15;

            // 1. EquipmentMenuTitle aranıyor, yoksa I tuşuna basılıp 100ms bekleniyor
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested) return;

                using (Bitmap? titleBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.EquipmentMenuTitlePosition))
                {
                    if (titleBmp != null)
                    {
                        var match = TemplateConstants.Match(titleBmp, TemplateConstants.WindowParts.EquipmentMenuTitle, threshold: 0.80);
                        if (match.IsSuccess)
                        {
                            Debug.WriteLine($"[StartupEquipmentMenu] EquipmentMenuTitle bulundu! (Deneme #{attempt}, Güven: %{match.Confidence * 100:F1})");
                            titleFound = true;
                            break;
                        }
                    }
                }

                // Bulunamadıysa I tuşuna bas ve 100ms bekle
                Debug.WriteLine($"[StartupEquipmentMenu] EquipmentMenuTitle bulunamadı, 'I' tuşuna basılıyor (Deneme #{attempt})...");
                BotLogger.LogKey(clientInfo.Id, "I (Envanter Menüsü Aç/Kapa)");
                byte iScan = (byte)Win32Native.MapVirtualKey(Win32Native.VK_I, 0);
                Win32Native.keybd_event((byte)Win32Native.VK_I, iScan, 0, 0);
                await Task.Delay(40, cancellationToken);
                Win32Native.keybd_event((byte)Win32Native.VK_I, iScan, Win32Native.KEYEVENTF_KEYUP, 0);

                await Task.Delay(100, cancellationToken);
            }

            if (!titleFound)
            {
                Debug.WriteLine($"[StartupEquipmentMenu] UYARI: {maxAttempts} denemede EquipmentMenuTitle bulunamadı.");
                return;
            }

            await Task.Delay(150, cancellationToken);

            // 2. EquipmentMenuExitButtonPosition alanında ExitButton şablonu aranıyor ve insansı kavisle tıklanıyor
            using (Bitmap? exitBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.EquipmentMenuExitButtonPosition))
            {
                int clickLocalX = RegionConstants.EquipmentMenuExitButtonPosition.StartX + (RegionConstants.EquipmentMenuExitButtonPosition.Width / 2);
                int clickLocalY = RegionConstants.EquipmentMenuExitButtonPosition.StartY + (RegionConstants.EquipmentMenuExitButtonPosition.Height / 2);

                if (exitBmp != null)
                {
                    var exitMatch = TemplateConstants.Match(exitBmp, TemplateConstants.WindowParts.EquipmentMenuExitButton, threshold: 0.75);
                    if (exitMatch.IsSuccess)
                    {
                        clickLocalX = RegionConstants.EquipmentMenuExitButtonPosition.StartX + exitMatch.Location.X + (exitMatch.Bounds.Width / 2);
                        clickLocalY = RegionConstants.EquipmentMenuExitButtonPosition.StartY + exitMatch.Location.Y + (exitMatch.Bounds.Height / 2);
                        Debug.WriteLine($"[StartupEquipmentMenu] EquipmentMenuExitButton tespit edildi! Konum: ({clickLocalX}, {clickLocalY}) Güven: %{exitMatch.Confidence * 100:F1}");
                    }
                }

                // Windows faresini insansı kavisle götür ve sol tıkla
                Debug.WriteLine($"[StartupEquipmentMenu] İnsansı fare hareketiyle Exit Button'a tıklanıyor: ({clickLocalX}, {clickLocalY})");
                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, clickLocalX, clickLocalY, fastMove: false, cancellationToken);
            }

            await Task.Delay(250, cancellationToken);
        }
    }
}
