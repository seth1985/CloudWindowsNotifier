using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace WindowsNotifierTray;

internal static class CoreSettingsStore
{
    private const string CoreSettingsKey = @"Software\CloudNotifier\Core";

    private static readonly Dictionary<string, int> Defaults = new()
    {
        ["Enabled"] = 1,
        ["PollingInterval"] = 300,
        ["AutoClearModules"] = 1,
        ["Heartbeat"] = 15,
        ["SoundEnabled"] = 1,
        ["ExitMenuVisible"] = 0,
        ["StartStopMenuVisible"] = 0
    };

    public static void SetSettings(Dictionary<string, int> settings)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(CoreSettingsKey);
            foreach (var kvp in settings)
            {
                key!.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
            }
        }
        catch
        {
        }
    }

    public static Dictionary<string, int> GetSettings()
    {
        var result = new Dictionary<string, int>(Defaults, StringComparer.OrdinalIgnoreCase);
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(CoreSettingsKey);
            foreach (var kvp in Defaults)
            {
                var value = key!.GetValue(kvp.Key) as int? ?? kvp.Value;
                key.SetValue(kvp.Key, value, RegistryValueKind.DWord);
                result[kvp.Key] = value;
            }
        }
        catch
        {
        }

        return result;
    }
}
