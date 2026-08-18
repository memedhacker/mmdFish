using System;
using System.Diagnostics;
using System.Drawing;

namespace Aether.Services
{
    /// <summary>
    /// Bot çalışma adımlarını, durum bildirimlerini ve tuş basımlarını UI (FishBotPage logPanel) ve konsola ileten merkezi loglayıcı.
    /// </summary>
    public static class BotLogger
    {
        /// <summary>
        /// Genel bot olayları ve işlem logları için event.
        /// </summary>
        public static event Action<int, string, Color>? OnLog;

        /// <summary>
        /// Yalnızca klavye tuş basımları (SPACE, Ctrl+G, 1, 2, F, G, I vb.) için özel log eventi.
        /// </summary>
        public static event Action<int, string, Color>? OnKeyLog;

        public static void Log(int clientId, string message, Color? color = null)
        {
            Color logColor = color ?? Color.FromArgb(220, 220, 225);
            Debug.WriteLine($"[BotLogger] [Client #{clientId}] {message}");
            OnLog?.Invoke(clientId, message, logColor);
        }

        public static void LogInfo(int clientId, string message) => Log(clientId, message, Color.FromArgb(120, 190, 255));
        public static void LogSuccess(int clientId, string message) => Log(clientId, message, Color.FromArgb(100, 230, 140));
        public static void LogWarning(int clientId, string message) => Log(clientId, message, Color.FromArgb(255, 205, 75));
        public static void LogError(int clientId, string message) => Log(clientId, message, Color.FromArgb(255, 100, 100));

        /// <summary>
        /// Basılan tuşları tuş log paneline kaydeder.
        /// </summary>
        public static void LogKey(int clientId, string keyName, Color? color = null)
        {
            Color keyColor = color ?? Color.FromArgb(245, 190, 80);
            Debug.WriteLine($"[KeyLogger] [Client #{clientId}] ⌨️ Tuş Basıldı: {keyName}");
            OnKeyLog?.Invoke(clientId, keyName, keyColor);
        }
    }
}
