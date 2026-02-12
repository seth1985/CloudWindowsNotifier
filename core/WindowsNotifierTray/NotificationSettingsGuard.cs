using Microsoft.Win32;

namespace WindowsNotifierTray;

internal static class NotificationSettingsGuard
{
    private const string NotificationsSettingsRoot = @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings";
    private static readonly string[] PreferredAumids = { "WindowsNotifier" };

    public static void EnsureEnabled()
    {
        try
        {
            using var settingsRoot = Registry.CurrentUser.CreateSubKey(NotificationsSettingsRoot, writable: true);
            if (settingsRoot == null)
            {
                return;
            }

            var targetKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var aumid in PreferredAumids)
            {
                targetKeys.Add(aumid);
            }

            foreach (var subKeyName in settingsRoot.GetSubKeyNames())
            {
                if (subKeyName.IndexOf("WindowsNotifier", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    targetKeys.Add(subKeyName);
                }
            }

            foreach (var keyName in targetKeys)
            {
                using var appKey = settingsRoot.CreateSubKey(keyName, writable: true);
                if (appKey == null)
                {
                    continue;
                }

                var enabled = ReadEnabled(appKey.GetValue("Enabled"));
                if (enabled == 0 || enabled == null)
                {
                    appKey.SetValue("Enabled", 1, RegistryValueKind.DWord);
                    Logger.Write("INFO", $"Notifications were disabled for '{keyName}'. Re-enabled.");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Write("WARN", $"Failed to enforce notification settings: {ex.Message}");
        }
    }

    private static int? ReadEnabled(object? value)
    {
        if (value is int i) return i;
        if (value is string s && int.TryParse(s, out var parsed)) return parsed;
        return null;
    }
}

