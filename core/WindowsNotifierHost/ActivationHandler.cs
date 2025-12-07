using System;
using System.Diagnostics;
using System.IO;

namespace WindowsNotifierHost;

internal static class ActivationHandler
{
    public static readonly string CoreRoot =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Windows Notifier", "Core");

    private static readonly string LogPath = Path.Combine(CoreRoot, "host-activation.log");

    internal static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(CoreRoot);
            var line = $"[{DateTime.Now:O}] {message}";
            File.AppendAllLines(LogPath, new[] { line });
        }
        catch
        {
            // logging must never throw
        }
    }

    public static void ForwardToCore(ActivationArgs args)
    {
        try
        {
            // Prefer installed core; fall back to project relative to host if needed.
            var coreScript = Path.Combine(CoreRoot, "WindowsNotifierCore.ps1");
            if (!File.Exists(coreScript))
            {
                var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                coreScript = Path.GetFullPath(Path.Combine(exeDir, @"..\..\core\WindowsNotifierCore.ps1"));
            }

            if (!File.Exists(coreScript))
            {
                Log("Core script not found; activation dropped.");
                return;
            }

            var psExe = ResolvePowerShell();
            if (psExe == null)
            {
                Log("Unable to resolve PowerShell executable; activation dropped.");
                return;
            }

            var activationArg = $"{args.Action}|{args.ModuleId}";
            Log($"Forwarding activation to core. CoreScript='{coreScript}', PSExe='{psExe}', ActivationArg='{activationArg}'");

            var psi = new ProcessStartInfo
            {
                FileName = psExe,
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList =
                {
                    "-NoLogo",
                    "-NoProfile",
                    "-ExecutionPolicy", "Bypass",
                    "-File", coreScript,
                    "-ActivationArgs", activationArg
                }
            };

            Process.Start(psi);
        }
        catch
        {
            Log("Unexpected error while forwarding activation to core.");
        }
    }

    private static string? ResolvePowerShell()
    {
        string systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.System);
        var legacy = Path.Combine(systemRoot, @"WindowsPowerShell\v1.0\powershell.exe");
        if (File.Exists(legacy))
        {
            return legacy;
        }

        return "powershell.exe";
    }
}
