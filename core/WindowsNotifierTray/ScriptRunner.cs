using System;
using System.Diagnostics;
using System.IO;

namespace WindowsNotifierTray;

internal sealed class ScriptResult
{
    public bool ShouldShow { get; set; }
    public string? Body { get; set; }
    public string? Error { get; set; }
}

internal static class ScriptRunner
{
    public static ScriptResult Run(string? scriptRelativePath, string moduleRoot)
    {
        if (string.IsNullOrWhiteSpace(scriptRelativePath))
        {
            return new ScriptResult { ShouldShow = true };
        }

        var scriptPath = Path.Combine(moduleRoot, scriptRelativePath);
        if (!File.Exists(scriptPath))
        {
            return new ScriptResult { ShouldShow = false, Error = $"Script not found: {scriptRelativePath}" };
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ResolvePowerShell(),
                WorkingDirectory = moduleRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-NoLogo");
            psi.ArgumentList.Add("-NoProfile");
            psi.ArgumentList.Add("-ExecutionPolicy");
            psi.ArgumentList.Add("Bypass");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(scriptPath);

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                return new ScriptResult { ShouldShow = false, Error = "Failed to start script process." };
            }

            var output = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit(30_000);

            if (proc.ExitCode != 0)
            {
                return new ScriptResult { ShouldShow = false, Error = $"Script exit code {proc.ExitCode}: {err}" };
            }

            return new ScriptResult { ShouldShow = true, Body = output.Trim() };
        }
        catch (Exception ex)
        {
            return new ScriptResult { ShouldShow = false, Error = ex.Message };
        }
    }

    private static string ResolvePowerShell()
    {
        var legacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"WindowsPowerShell\v1.0\powershell.exe");
        if (File.Exists(legacy)) return legacy;
        return "powershell.exe";
    }
}
