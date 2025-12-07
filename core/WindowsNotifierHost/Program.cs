using System;
using System.IO;
using System.Threading;
using Microsoft.Toolkit.Uwp.Notifications;

namespace WindowsNotifierHost;

internal static class Program
{
    private static ManualResetEventSlim _activatedEvent = new(false);

    [STAThread]
    private static void Main(string[] args)
    {
        var coreRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Windows Notifier",
            "Core");
        var logPath = Path.Combine(coreRoot, "host-activation.log");

        void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(coreRoot);
                var line = $"[{DateTime.Now:O}] {message}";
                File.AppendAllLines(logPath, new[] { line });
            }
            catch
            {
                // never throw from host logging
            }
        }

        string invokedArgs = string.Join(" ", args ?? Array.Empty<string>());
        Log($"Host launched. RawArgs='{invokedArgs}'");

        // If launched via direct arguments, parse and forward immediately.
        if (!string.IsNullOrWhiteSpace(invokedArgs) && !string.Equals(invokedArgs, "-Embedding", StringComparison.OrdinalIgnoreCase))
        {
            var parsed = ActivationArgs.Parse(invokedArgs);
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.ModuleId))
            {
                Log("Failed to parse activation arguments or missing module id (direct launch).");
            }
            else
            {
                Log($"Parsed activation (direct). Action='{parsed.Action}', Module='{parsed.ModuleId}', Url='{parsed.Url ?? ""}'");
                ActivationHandler.ForwardToCore(parsed);
                return;
            }
        }

        // If launched for COM activation (-Embedding) wait briefly in case callbacks need it.
        if (string.Equals(invokedArgs, "-Embedding", StringComparison.OrdinalIgnoreCase))
        {
            Log("Launched with -Embedding; waiting briefly.");
            _activatedEvent.Wait(TimeSpan.FromSeconds(10));
        }
    }
}
