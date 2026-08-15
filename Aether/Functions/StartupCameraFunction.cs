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
    /// Bot başlangıcında kamera açısını, pencere odağını ve F / G tuş sekansını yöneten modül.
    /// </summary>
    public static class StartupCameraFunction
    {
        /// <summary>
        /// F ve G tuşlarına sırayla 3'er saniye basılı tutup bırakarak kamera açısını hazırlar.
        /// </summary>
        public static async Task ExecuteCameraPreparationAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || cancellationToken.IsCancellationRequested) return;

            // 1. Sırayla önce F tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[StartupCamera] Client #{clientInfo.Id} -> 'F' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_F, 3000, cancellationToken);
            Debug.WriteLine($"[StartupCamera] Client #{clientInfo.Id} -> 'F' tuşu bırakıldı.");

            await Task.Delay(300, cancellationToken);

            // 2. Sonra G tuşuna 3 saniye basılı tutup bırakacak
            Debug.WriteLine($"[StartupCamera] Client #{clientInfo.Id} -> 'G' tuşuna 3 saniye basılı tutuluyor...");
            await HoldKeyAsync(Win32Native.VK_G, 3000, cancellationToken);
            Debug.WriteLine($"[StartupCamera] Client #{clientInfo.Id} -> 'G' tuşu bırakıldı.");
        }

        /// <summary>
        /// Belirtilen sanal tuşa (VK) verilen süre boyunca basılı tutar.
        /// Oyun motorlarının (DirectX / DirectInput) tuş basılı tutma durumunu kesintisiz algılaması için
        /// periyodik donanımsal tekrar (typematic repeat) darbeleri üretir ve süre bitiminde tuşu serbest bırakır.
        /// </summary>
        public static async Task HoldKeyAsync(uint vk, int durationMs, CancellationToken cancellationToken)
        {
            byte scanCode = (byte)Win32Native.MapVirtualKey(vk, 0);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // İlk tuş basma (Key Down)
                Win32Native.keybd_event((byte)vk, scanCode, 0, 0);

                while (stopwatch.ElapsedMilliseconds < durationMs)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    // DirectInput / Oyun döngülerinin tuşun basılı kaldığını sürekli hissetmesi için repeat sinyali
                    Win32Native.keybd_event((byte)vk, scanCode, 0, 0);
                    await Task.Delay(40, cancellationToken);
                }
            }
            finally
            {
                // Tuşu garanti olarak serbest bırak (Key Up)
                Win32Native.keybd_event((byte)vk, scanCode, Win32Native.KEYEVENTF_KEYUP, 0);
            }
        }

        /// <summary>
        /// Verilen pencerenin merkez koordinatını hesaplayıp fareyi insansı kavisle oraya taşır ve sağ tıklar.
        /// </summary>
        public static async Task PerformWindowCenterRightClickAsync(IntPtr hWnd, CancellationToken cancellationToken)
        {
            try
            {
                if (hWnd == IntPtr.Zero || !Win32Native.IsWindow(hWnd)) return;

                int localX = 400;
                int localY = 300;

                if (Win32Native.GetClientRect(hWnd, out Win32Native.RECT clientRect) && clientRect.Width > 0 && clientRect.Height > 0)
                {
                    localX = clientRect.Width / 2;
                    localY = clientRect.Height / 2;
                }

                Debug.WriteLine($"[StartupCamera] Pencere merkezine insansı fare ile sağ tıklanıyor: ({localX}, {localY})");
                await HumanMouseService.Instance.RightClickLocalAsync(hWnd, localX, localY, fastMove: false, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StartupCamera] Sağ tık hatası: {ex.Message}");
            }
        }
    }
}
