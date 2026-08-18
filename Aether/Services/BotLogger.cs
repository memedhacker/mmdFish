using System;
using System.Diagnostics;
using System.Drawing;

namespace Aether.Services
{
    /// <summary>
    /// Bot çalışma adımlarını ve durum bildirimlerini UI (FishBotPage logPanel) ve konsola ileten merkezi loglayıcı.
    /// </summary>
    public static class BotLogger
    {
        public static event Action<int, string, Color>? OnLog;

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
    }
}
