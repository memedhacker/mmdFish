using Aether.Constants;
using Aether.Helpers;
using Aether.Models;
using Aether.Services;
using Aether.States;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Aether.Functions
{
    /// <summary>
    /// Envanter sayfası durumunu (Sayfa 1, 2, 3, 4) tespit eden ve istemci ayarlarındaki hedef sayfayı aktif kılan modül.
    /// </summary>
    public static class StartupInventoryPageFunction
    {
        /// <summary>
        /// InventoryPagesPosition alanında 1, 2, 3, 4 sayfalarının açık/kapalı durumunu tespit eder.
        /// Client ayarlarında tanımlı hedef sayfa (InventoryPage) seçili değilse,
        /// Windows faresini insansı kavisle ilgili sayfa butonuna götürüp tıklar.
        /// </summary>
        public static async Task EnsureInventoryPageSelectedAsync(ClientInfo clientInfo, CancellationToken cancellationToken)
        {
            if (clientInfo == null || clientInfo.Handle == IntPtr.Zero || cancellationToken.IsCancellationRequested) return;

            // İstemcinin kayıtlı balık botu ayarlarından hedef sayfa numarasını al (1, 2, 3 veya 4)
            var settings = FishBotSettingsRegistry.Instance.GetOrCreate(clientInfo.Id);
            int targetPage = Math.Clamp(settings?.InventoryPage ?? 1, 1, 4);

            Debug.WriteLine($"[StartupInventoryPage] Client #{clientInfo.Id} -> Hedef Envanter Sayfası: {targetPage}. Sayfa durumu taranıyor...");

            using (Bitmap? pagesBmp = WindowRegionCaptureHelper.CaptureRegion(clientInfo.Handle, RegionConstants.InventoryPagesPosition))
            {
                if (pagesBmp == null)
                {
                    Debug.WriteLine("[StartupInventoryPage] InventoryPagesPosition ekran görüntüsü alınamadı.");
                    return;
                }

                // Açık/Aktif olan sayfa şablonları
                string[] activeTemplates = {
                    TemplateConstants.WindowParts.Page1Acik,
                    TemplateConstants.WindowParts.Page2Acik,
                    TemplateConstants.WindowParts.Page3Acik,
                    TemplateConstants.WindowParts.Page4Acik
                };

                // Kapalı/Pasif olan sayfa şablonları
                string[] closedTemplates = {
                    TemplateConstants.WindowParts.Page1,
                    TemplateConstants.WindowParts.Page2,
                    TemplateConstants.WindowParts.Page3,
                    TemplateConstants.WindowParts.Page4
                };

                int currentlyActivePage = -1;
                double maxActiveConfidence = 0;

                // 1. Hangi sayfanın şu an açık olduğunu tespit et
                for (int p = 0; p < 4; p++)
                {
                    var activeMatch = TemplateConstants.Match(pagesBmp, activeTemplates[p], threshold: 0.70);
                    if (activeMatch.IsSuccess && activeMatch.Confidence > maxActiveConfidence)
                    {
                        maxActiveConfidence = activeMatch.Confidence;
                        currentlyActivePage = p + 1;
                    }
                }

                Debug.WriteLine($"[StartupInventoryPage] Tespit edilen aktif sayfa: {(currentlyActivePage > 0 ? currentlyActivePage.ToString() : "Bilinmiyor")} (Güven: %{maxActiveConfidence * 100:F1})");

                // Eğer hedeflenen sayfa zaten açıksa hiçbir işlem yapma
                if (currentlyActivePage == targetPage)
                {
                    Debug.WriteLine($"[StartupInventoryPage] Sayfa {targetPage} zaten aktif durumda, tıklamaya gerek yok.");
                    return;
                }

                // 2. Hedef sayfa kapalıysa butonun konumunu tespit et
                string targetClosedTemplate = closedTemplates[targetPage - 1];
                var buttonMatch = TemplateConstants.Match(pagesBmp, targetClosedTemplate, threshold: 0.70);

                int clickLocalX;
                // Sayfa butonlarının dikey eksende kesinlikle tam ortasına basması için bölge merkezini sabitle
                int clickLocalY = RegionConstants.InventoryPagesPosition.StartY + (RegionConstants.InventoryPagesPosition.Height / 2);

                if (buttonMatch.IsSuccess)
                {
                    clickLocalX = RegionConstants.InventoryPagesPosition.StartX + buttonMatch.Location.X + (buttonMatch.Bounds.Width / 2);
                    Debug.WriteLine($"[StartupInventoryPage] Sayfa {targetPage} butonu OpenCV ile bulundu: ({clickLocalX}, {clickLocalY}) Güven: %{buttonMatch.Confidence * 100:F1}");
                }
                else
                {
                    // Fallback: 4 buton için yatay orantısal merkez hesaplama (1..4)
                    int regionWidth = RegionConstants.InventoryPagesPosition.Width;
                    int segmentWidth = regionWidth / 4;
                    clickLocalX = RegionConstants.InventoryPagesPosition.StartX + (segmentWidth * (targetPage - 1)) + (segmentWidth / 2);
                    Debug.WriteLine($"[StartupInventoryPage] Sayfa {targetPage} şablonu tam eşleşmedi, orantısal koordinata tıklanacak: ({clickLocalX}, {clickLocalY})");
                }

                // 3. Windows faresini insansı kavisle tam ortaya götür ve garanti olması için 3-6 defa rastgele tıkla
                int clickCount = Random.Shared.Next(3, 7); // 3, 4, 5 veya 6 defa
                Debug.WriteLine($"[StartupInventoryPage] İnsansı fare hareketiyle Sayfa {targetPage} butonunun tam merkezine ({clickLocalX}, {clickLocalY}) {clickCount} kez tıklanıyor...");
                await HumanMouseService.Instance.LeftClickLocalAsync(clientInfo.Handle, clickLocalX, clickLocalY, fastMove: false, clickCount: clickCount, cancellationToken: cancellationToken);
            }

            await Task.Delay(250, cancellationToken);
        }
    }
}
