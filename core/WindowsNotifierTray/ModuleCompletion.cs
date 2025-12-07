using System;
using System.IO;

namespace WindowsNotifierTray;

internal static class ModuleCompletion
{
    public static void Finish(string moduleId, string modulePath, ModuleState? previous, DateTime nowUtc, bool autoClear)
    {
        ModuleStateStore.SetState(moduleId, new ModuleState
        {
            Status = "Completed",
            UserDismissed = 1,
            AcknowledgedAt = nowUtc,
            LastError = null,
            LastShownAt = previous?.LastShownAt,
            ScheduledAt = previous?.ScheduledAt,
            ReminderDueAt = previous?.ReminderDueAt,
            ReminderHours = previous?.ReminderHours
        });

        if (autoClear)
        {
            try
            {
                if (Directory.Exists(modulePath))
                {
                    Directory.Delete(modulePath, true);
                    Logger.Write("INFO", $"Auto-cleared module folder for '{moduleId}'.");
                }
            }
            catch { }
        }
    }
}

