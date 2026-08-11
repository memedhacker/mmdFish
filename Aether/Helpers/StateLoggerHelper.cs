using Aether.Models;
using Aether.States;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Aether.Helpers
{
    /// <summary>
    /// Test amaçlı olarak tüm state verilerini (ClientState, PageState, FishBotSettingsRegistry)
    /// detaylı ve anlamlı bir Markdown raporu şeklinde Masaüstüne (log.md) yazdıran yardımcı sınıf.
    /// </summary>
    public static class StateLoggerHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Tüm uygulama state'lerini masaüstündeki log.md dosyasına yazar.
        /// </summary>
        /// <returns>Oluşturulan dosyanın tam yolu.</returns>
        public static string ExportAllStatesToDesktop()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "log.md");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("# 🔍 Uygulama State Raporu (System Log Dump)");
            sb.AppendLine($"**Oluşturulma Tarihi:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // 1. PAGE STATE
            sb.AppendLine("## 📄 1. Page State");
            sb.AppendLine($"- **Current Page:** `{PageState.Instance.CurrentPage}`");
            sb.AppendLine();

            // 2. CLIENT STATE
            sb.AppendLine("## 👥 2. Client State");
            var selected = ClientState.Instance.SelectedClient;
            if (selected != null)
            {
                sb.AppendLine($"### 🎯 Aktif Seçili Client");
                sb.AppendLine($"- **ID:** `{selected.Id}`");
                sb.AppendLine($"- **Name:** `{selected.Name}`");
                sb.AppendLine($"- **HWND:** `0x{selected.Handle.ToInt64():X}` ({selected.Handle})");
                sb.AppendLine($"- **PID:** `{selected.ProcessId}`");
            }
            else
            {
                sb.AppendLine("- **Aktif Seçili Client:** *(Yok)*");
            }
            sb.AppendLine();

            sb.AppendLine($"### 📋 İşaretli (Checked) Client Listesi ({ClientState.Instance.CheckedClients.Count} Adet)");
            if (ClientState.Instance.CheckedClients.Count > 0)
            {
                foreach (var c in ClientState.Instance.CheckedClients)
                {
                    sb.AppendLine($"- **Client #{c.Id}:** {c.Name} | HWND: `0x{c.Handle.ToInt64():X}` | PID: `{c.ProcessId}`");
                }
            }
            else
            {
                sb.AppendLine("- *(Hiçbir client işaretlenmemiş)*");
            }
            sb.AppendLine();

            // 3. FISHBOT SETTINGS REGISTRY
            sb.AppendLine("## 🐟 3. FishBot Settings Registry (Per-Client)");
            for (int id = 1; id <= 10; id++)
            {
                bool has = FishBotSettingsRegistry.Instance.HasSettings(id);
                sb.AppendLine($"### Client #{id} Settings {(has ? "✅ (Kayıtlı)" : "⚠️ (Varsayılan Nesne)")}");

                var settings = FishBotSettingsRegistry.Instance.GetOrCreate(id);
                sb.AppendLine("```json");
                try
                {
                    sb.AppendLine(JsonSerializer.Serialize(settings, JsonOptions));
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"// JSON Serialize Hatası: {ex.Message}");
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return filePath;
        }
    }
}
