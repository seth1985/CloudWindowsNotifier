using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace WindowsNotifierTray;

internal static class CoreStateStore
{
    private const string CoreKey = @"Software\WindowsNotifier\Core";

    public static void SetState(Dictionary<string, object?> values)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(CoreKey);
            foreach (var kvp in values)
            {
                if (kvp.Value == null)
                {
                    try { key!.DeleteValue(kvp.Key, false); } catch { }
                    continue;
                }

                if (kvp.Value is int i)
                {
                    key!.SetValue(kvp.Key, i, RegistryValueKind.DWord);
                }
                else
                {
                    key!.SetValue(kvp.Key, kvp.Value.ToString() ?? string.Empty);
                }
            }
        }
        catch
        {
        }
    }  
}
