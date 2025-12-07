using System;
using System.IO;

namespace WindowsNotifierTray;

internal static class Logger
{
    private static readonly object Sync = new();

    private static string LogPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Windows Notifier",
        "Core",
        "core.log");

    public static void Write(string level, string message)
    {
        try
        {
            var root = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(root) && !Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            var line = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss} [{level.ToUpperInvariant(),-5}] {message}";
            lock (Sync)
            {
                File.AppendAllLines(LogPath, new[] { line });
            }
        }
        catch
        {
            // logging must never throw
        }
    }
}

