using Microsoft.Win32;
using System;

namespace WindowsNotifierTray;

public sealed class ModuleState
{
    public string Status { get; set; } = "Pending";
    public int UserDismissed { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? LastError { get; set; }
    public string? LastShownAt { get; set; }
    public string? ScheduledAt { get; set; }
    public string? ReminderDueAt { get; set; }
    public string? ReminderHours { get; set; }
    public string? LastConditionCheckAt { get; set; }
    public string? NextConditionCheckAt { get; set; }
}

internal static class ModuleStateStore
{
    private const string RootKey = @"Software\CloudNotifier\Modules";

    public static ModuleState? GetState(string moduleId)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey($@"{RootKey}\{moduleId}");
            if (key == null) return null;

            var status = key.GetValue("Status") as string ?? "Pending";
            var userDismissed = key.GetValue("UserDismissed") as int? ?? 0;
            var ackString = key.GetValue("AcknowledgedAt") as string;
            DateTime? ack = null;
            if (DateTime.TryParse(ackString, out var parsed))
            {
                ack = parsed;
            }
            var lastError = key.GetValue("LastError") as string;
            var lastShown = key.GetValue("LastShownAt") as string;
            var scheduled = key.GetValue("ScheduledAt") as string;
            var reminderDue = key.GetValue("ReminderDueAt") as string;
            var reminderHours = key.GetValue("ReminderHours") as string;
            var lastCond = key.GetValue("LastConditionCheckAt") as string;
            var nextCond = key.GetValue("NextConditionCheckAt") as string;

            return new ModuleState
            {
                Status = status,
                UserDismissed = userDismissed,
                AcknowledgedAt = ack,
                LastError = lastError,
                LastShownAt = lastShown,
                ScheduledAt = scheduled,
                ReminderDueAt = reminderDue,
                ReminderHours = reminderHours,
                LastConditionCheckAt = lastCond,
                NextConditionCheckAt = nextCond
            };
        }
        catch
        {
            return null;
        }
    }

    public static void SetState(string moduleId, ModuleState state)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey($@"{RootKey}\{moduleId}");
            key.SetValue("Status", state.Status);
            key.SetValue("UserDismissed", state.UserDismissed);

            if (state.AcknowledgedAt.HasValue)
            {
                key.SetValue("AcknowledgedAt", state.AcknowledgedAt.Value.ToString("o"));
            }
            else
            {
                try { key.DeleteValue("AcknowledgedAt", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.LastError))
            {
                key.SetValue("LastError", state.LastError);
            }
            else
            {
                try { key.DeleteValue("LastError", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.LastShownAt))
            {
                key.SetValue("LastShownAt", state.LastShownAt);
            }
            else
            {
                try { key.DeleteValue("LastShownAt", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.ScheduledAt))
            {
                key.SetValue("ScheduledAt", state.ScheduledAt);
            }
            else
            {
                try { key.DeleteValue("ScheduledAt", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.ReminderDueAt))
            {
                key.SetValue("ReminderDueAt", state.ReminderDueAt);
            }
            else
            {
                try { key.DeleteValue("ReminderDueAt", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.ReminderHours))
            {
                key.SetValue("ReminderHours", state.ReminderHours);
            }
            else
            {
                try { key.DeleteValue("ReminderHours", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.LastConditionCheckAt))
            {
                key.SetValue("LastConditionCheckAt", state.LastConditionCheckAt);
            }
            else
            {
                try { key.DeleteValue("LastConditionCheckAt", false); } catch { }
            }

            if (!string.IsNullOrEmpty(state.NextConditionCheckAt))
            {
                key.SetValue("NextConditionCheckAt", state.NextConditionCheckAt);
            }
            else
            {
                try { key.DeleteValue("NextConditionCheckAt", false); } catch { }
            }
        }
        catch
        {
            // ignore
        }
    }
}
