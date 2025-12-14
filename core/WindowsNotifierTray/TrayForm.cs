using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.Json;
using System.Collections.Generic;
using Windows.UI.Notifications;

namespace WindowsNotifierTray;

public class TrayForm : Form
{
    static TrayForm()
    {
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            HandleActivation(toastArgs.Argument);
        };
    }

    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _startItem;
    private readonly ToolStripMenuItem _stopItem;
    private readonly ToolStripMenuItem _refreshItem;
    private readonly ToolStripMenuItem _exitItem;
    private System.Collections.Generic.Dictionary<string, int>? _settings;
    private readonly System.Windows.Forms.Timer _scanTimer;
    private int _pollingIntervalSeconds = 60;
    private readonly Dictionary<string, string> _presetIconCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _defaultLogoPath;
    private static readonly TelemetryClient _telemetry = CreateTelemetryClient();

    private string InstallRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CloudNotifier",
        "Core");

    private static string ModulesRootDefault => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CloudNotifier",
        "Modules");

    public TrayForm()
    {
        _startItem = new ToolStripMenuItem("Start", null, (_, _) => StartCoreLoop());
        _stopItem = new ToolStripMenuItem("Stop", null, (_, _) => StopCore());
        _refreshItem = new ToolStripMenuItem("Refresh", null, (_, _) => RefreshCore());
        _exitItem = new ToolStripMenuItem("Exit", null, (_, _) => ExitApp());

        var menu = new ContextMenuStrip();
        menu.Items.AddRange(new ToolStripItem[]
        {
            _startItem,
            _stopItem,
            _refreshItem,
            new ToolStripSeparator(),
            _exitItem
        });

        _tray = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Information,
            Text = "Cloud Notifier",
            Visible = true,
            ContextMenuStrip = menu
        };

        _scanTimer = new System.Windows.Forms.Timer { Interval = 60_000 };
        _scanTimer.Tick += (_, _) => RunScan();

        _defaultLogoPath = EnsureDefaultLogo();
        Visible = false;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        Load += (_, _) => UpdateMenuState();
    }

    public void StartCoreLoop()
    {
        try
        {
            StopCore();
            Logger.Write("INFO", "Starting tray scan loop.");
            RunScan();
            _scanTimer.Interval = Math.Max(15_000, _pollingIntervalSeconds * 1000);
            _scanTimer.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start scan: {ex.Message}", "Cloud Notifier", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Logger.Write("ERROR", $"Failed to start scan: {ex.Message}");
        }
        finally
        {
            UpdateMenuState();
        }
    }

    private void StopCore()
    {
        _scanTimer.Stop();
        Logger.Write("INFO", "Scan loop stopped.");
        UpdateMenuState();

    }

    private void RefreshCore()
    {
        StopCore();
        StartCoreLoop();
    }

    private void ExitApp()
    {
        StopCore();
        _tray.Visible = false;
        Application.Exit();
    }

    private void UpdateMenuState()
    {
        bool running = _scanTimer.Enabled;
        _startItem.Enabled = !running;
        _stopItem.Enabled = running;
        _refreshItem.Enabled = true;

        var exitVisible = true;
        if (_settings != null && _settings.TryGetValue("ExitMenuVisible", out var vis))
        {
            exitVisible = vis != 0;
        }
        _exitItem.Visible = exitVisible;

        var startStopVisible = true;
        if (_settings != null && _settings.TryGetValue("StartStopMenuVisible", out var ss))
        {
            startStopVisible = ss != 0;
        }
        _startItem.Visible = startStopVisible;
        _stopItem.Visible = startStopVisible;

        var statusText = running ? "Running" : "Stopped";
        _tray.Text = $"Cloud Notifier - {statusText}";
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Hide();
    }

    private static string ResolvePowerShell()
    {
        var legacy = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"WindowsPowerShell\v1.0\powershell.exe");
        if (File.Exists(legacy)) return legacy;
        return "powershell.exe";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tray.Dispose();
            _scanTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    private void RunScan()
    {
        _settings = CoreSettingsStore.GetSettings();
        _pollingIntervalSeconds = _settings.TryGetValue("PollingInterval", out var poll) ? poll : 60;
        var autoClearSetting = _settings.TryGetValue("AutoClearModules", out var acSetting) ? acSetting : 1;
        _scanTimer.Interval = Math.Max(15_000, _pollingIntervalSeconds * 1000);
        UpdateMenuState();

        var modulesRoot = ModulesRootDefault;
        if (!Directory.Exists(modulesRoot))
        {
            Directory.CreateDirectory(modulesRoot);
            return;
        }

        var now = DateTime.UtcNow;
        Logger.Write("DEBUG", $"Starting module scan in '{modulesRoot}'.");
        int total = 0;
        int shown = 0;
        int errors = 0;

        foreach (var dir in Directory.GetDirectories(modulesRoot))
        {
            total++;
            var manifestPath = Path.Combine(dir, "manifest.json");
            if (!File.Exists(manifestPath)) continue;

            ModuleManifest? manifest = null;
            try
            {
                var json = File.ReadAllText(manifestPath);
                manifest = JsonSerializer.Deserialize<ModuleManifest>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch { }

            if (manifest == null || string.IsNullOrWhiteSpace(manifest.id)) continue;

            var state = ModuleStateStore.GetState(manifest.id);
            var status = state?.Status ?? "Pending";
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase))
            {
                Logger.Write("DEBUG", $"Skipping '{manifest.id}' due to status '{status}'.");
                continue;
            }

            // Core settings modules: apply and clear
        if (string.Equals(manifest.type, "core_update", StringComparison.OrdinalIgnoreCase) && manifest.core_settings != null)
        {
            var settingsToApply = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (manifest.core_settings.enabled.HasValue) settingsToApply["Enabled"] = manifest.core_settings.enabled.Value;
            if (manifest.core_settings.polling_interval_seconds.HasValue) settingsToApply["PollingInterval"] = manifest.core_settings.polling_interval_seconds.Value;
            if (manifest.core_settings.auto_clear_modules.HasValue) settingsToApply["AutoClearModules"] = manifest.core_settings.auto_clear_modules.Value;
            if (manifest.core_settings.heartbeat_seconds.HasValue) settingsToApply["Heartbeat"] = manifest.core_settings.heartbeat_seconds.Value;
            if (manifest.core_settings.sound_enabled.HasValue) settingsToApply["SoundEnabled"] = manifest.core_settings.sound_enabled.Value;
            if (manifest.core_settings.exit_menu_visible.HasValue) settingsToApply["ExitMenuVisible"] = manifest.core_settings.exit_menu_visible.Value;
            if (manifest.core_settings.start_stop_menu_visible.HasValue) settingsToApply["StartStopMenuVisible"] = manifest.core_settings.start_stop_menu_visible.Value;

            if (settingsToApply.Count > 0)
            {
                CoreSettingsStore.SetSettings(settingsToApply);
                Logger.Write("INFO", $"Applied core settings from module '{manifest.id}'.");
                _ = _telemetry.SendAsync(manifest.id, "CoreSettingsApplied", new Dictionary<string, object>
                {
                    ["enabled"] = manifest.core_settings.enabled ?? 0,
                    ["polling"] = manifest.core_settings.polling_interval_seconds ?? 0,
                    ["heartbeat"] = manifest.core_settings.heartbeat_seconds ?? 0,
                    ["autoClear"] = manifest.core_settings.auto_clear_modules ?? 0,
                    ["sound"] = manifest.core_settings.sound_enabled ?? 0,
                    ["exitMenu"] = manifest.core_settings.exit_menu_visible ?? 0,
                    ["startStopMenu"] = manifest.core_settings.start_stop_menu_visible ?? 0
                });
            }

            ModuleCompletion.Finish(manifest.id, dir, state, now, autoClearSetting == 1);
            shown++;
            continue;
        }

        // Conditional modules
        if (string.Equals(manifest.type, "conditional", StringComparison.OrdinalIgnoreCase))
        {
            var condScript = manifest.behavior?.conditional_script ?? manifest.script;
            var cond = ScriptRunner.Run(condScript, dir);
            if (cond.Error != null)
            {
                errors++;
                ModuleStateStore.SetState(manifest.id, new ModuleState
                {
                    Status = "Error",
                    LastError = cond.Error
                });
                continue;
            }

            var stateUpdate = new ModuleState
            {
                Status = "Pending",
                LastConditionCheckAt = now.ToString("o"),
                LastShownAt = state?.LastShownAt,
                ScheduledAt = state?.ScheduledAt,
                ReminderDueAt = state?.ReminderDueAt,
                ReminderHours = state?.ReminderHours
            };

            var intervalMinutes = manifest.behavior?.conditional_interval_minutes ?? 0;

            if (!cond.ShouldShow)
            {
                if (intervalMinutes > 0)
                {
                    var nextCheck = now.AddMinutes(intervalMinutes);
                    stateUpdate.NextConditionCheckAt = nextCheck.ToString("o");
                    Logger.Write("DEBUG", $"Conditional '{manifest.id}' not shown; next check at {stateUpdate.NextConditionCheckAt}.");
                }
                ModuleStateStore.SetState(manifest.id, stateUpdate);
                continue;
            }

            ModuleStateStore.SetState(manifest.id, stateUpdate);
        }

        // Reminder/scheduling logic
        var reminderHours = manifest.behavior?.reminder_hours ?? ParseInt(state?.ReminderHours);
            var reminderDue = ParseDate(state?.ReminderDueAt);
            var lastShown = ParseDate(state?.LastShownAt);
            bool shouldShow = false;

            var scheduleUtc = ParseDate(manifest.schedule_utc) ?? ParseDate(manifest.created_utc) ?? now;
            var expiresUtc = ParseDate(manifest.expires_utc);

            if (state?.UserDismissed == 1)
            {
                Logger.Write("DEBUG", $"Skipping '{manifest.id}' because user dismissed.");
                continue;
            }

            if (expiresUtc.HasValue && now >= expiresUtc.Value)
            {
                ModuleStateStore.SetState(manifest.id, new ModuleState
                {
                    Status = "Expired",
                    LastError = null,
                    LastShownAt = state?.LastShownAt
                });

                Logger.Write("INFO", $"Marked '{manifest.id}' as Expired (now={now:o}, expires={expiresUtc.Value:o}).");

                if (autoClearSetting == 1)
                {
                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true);
                            Logger.Write("INFO", $"Auto-cleared expired module folder for '{manifest.id}'.");
                        }
                    }
                    catch { }
                }

                continue;
            }

        if (reminderDue.HasValue)
        {
            shouldShow = now >= reminderDue.Value;
        }
            else if (!lastShown.HasValue)
            {
                shouldShow = now >= scheduleUtc;
            }
            else if (reminderHours.HasValue && reminderHours.Value > 0)
            {
                shouldShow = now >= lastShown.Value.AddHours(reminderHours.Value);
            }
            else
            {
                shouldShow = true;
            }

            if (!shouldShow)
            {
                Logger.Write("DEBUG", $"Skipping '{manifest.id}' (now={now:o}, scheduleUtc={scheduleUtc:o}, lastShown={lastShown}, reminderDue={reminderDue}, reminderHours={reminderHours}).");
                continue;
            }

            // Dynamic modules: body from script
            if (string.Equals(manifest.type, "dynamic", StringComparison.OrdinalIgnoreCase))
            {
                var dynScript = manifest.dynamic?.script ?? manifest.script;
                var dynResult = ScriptRunner.Run(dynScript, dir);
                if (dynResult.Error != null)
                {
                    errors++;
                    ModuleStateStore.SetState(manifest.id, new ModuleState
                    {
                        Status = "Error",
                        LastError = dynResult.Error
                    });
                    Logger.Write("ERROR", $"Dynamic '{manifest.id}' script failed: {dynResult.Error}");
                    continue;
                }

                var body = dynResult.Body ?? manifest.message ?? string.Empty;
                var opts = manifest.dynamic?.options;
                if (opts?.trim_whitespace == true)
                {
                    body = body?.Trim();
                }
                if (opts?.fail_if_empty == true && string.IsNullOrWhiteSpace(body))
                {
                    errors++;
                    ModuleStateStore.SetState(manifest.id, new ModuleState
                    {
                        Status = "Error",
                        LastError = "Dynamic script returned empty content."
                    });
                    continue;
                }
                if (opts?.fallback_message != null && string.IsNullOrWhiteSpace(body))
                {
                    body = opts.fallback_message;
                }

                // Enforce max length: use manifest value if present, otherwise hard-cap at 160 (toast body limit).
                var maxLength = 160;
                if (manifest.dynamic?.max_length.HasValue == true && manifest.dynamic.max_length.Value > 0)
                {
                    maxLength = Math.Min(maxLength, manifest.dynamic.max_length.Value);
                }
                if (!string.IsNullOrEmpty(body) && body.Length > maxLength)
                {
                    body = body.Substring(0, maxLength);
                }

                var toastShown = ShowToast(manifest, dir, body);
                if (toastShown)
            {
                shown++;
                var nowUtc = now.ToString("o");
                var nextDue = (reminderHours.HasValue && reminderHours.Value > 0)
                    ? now.AddHours(reminderHours.Value).ToString("o")
                    : null;
                ModuleStateStore.SetState(manifest.id, new ModuleState
                {
                    Status = reminderHours.HasValue && reminderHours.Value > 0 ? "Pending" : "Completed",
                    UserDismissed = state?.UserDismissed ?? 0,
                    LastShownAt = nowUtc,
                    ScheduledAt = nowUtc,
                    ReminderDueAt = nextDue,
                    ReminderHours = manifest.behavior?.reminder_hours?.ToString(),
                    LastError = null,
                    AcknowledgedAt = state?.AcknowledgedAt
                    });
                }
                else
                {
                    errors++;
                    Logger.Write("WARN", $"Toast skipped or failed for dynamic module '{manifest.id}'.");
                }
                continue;
            }

        var standardToast = ShowToast(manifest, dir);
        if (standardToast)
        {
            shown++;
            _ = _telemetry.SendAsync(manifest.id, "ToastShown", new Dictionary<string, object> { ["type"] = manifest.type ?? "standard" });
            var nowUtc = now.ToString("o");
            var nextDue = (reminderHours.HasValue && reminderHours.Value > 0)
                ? now.AddHours(reminderHours.Value).ToString("o")
                : null;
            ModuleStateStore.SetState(manifest.id, new ModuleState
            {
                Status = reminderHours.HasValue && reminderHours.Value > 0 ? "Pending" : "Completed",
                UserDismissed = state?.UserDismissed ?? 0,
                LastShownAt = nowUtc,
                ScheduledAt = nowUtc,
                ReminderDueAt = nextDue,
                ReminderHours = manifest.behavior?.reminder_hours?.ToString(),
                LastError = null,
                AcknowledgedAt = state?.AcknowledgedAt
            });
        }
        else
        {
            errors++;
            Logger.Write("WARN", $"Toast skipped or failed for '{manifest.id}'.");
        }
        }

        CoreStateStore.SetState(new System.Collections.Generic.Dictionary<string, object?>
        {
            ["LastScanAt"] = DateTime.UtcNow.ToString("o"),
            ["ModulesRoot"] = modulesRoot,
            ["TotalModules"] = total,
            ["ToastsShown"] = shown,
            ["ErrorModules"] = errors
        });
        Logger.Write("INFO", $"Scan complete. Total={total}, Shown={shown}, Errors={errors}, ModulesRoot={modulesRoot}");
    }

    private bool ShowToast(ModuleManifest manifest, string moduleRoot, string? bodyOverride = null)
    {
        var isHero = string.Equals(manifest.type, "hero", StringComparison.OrdinalIgnoreCase);
        var title = Clamp(string.IsNullOrWhiteSpace(manifest.title) ? "Cloud Notifier" : manifest.title, 60);
        var body = Clamp(bodyOverride ?? manifest.message ?? string.Empty, 160);
        var reminderHours = manifest.behavior?.reminder_hours;
        var hasReminder = reminderHours.HasValue && reminderHours.Value > 0;
        var soundEnabledSetting = _settings != null && _settings.TryGetValue("SoundEnabled", out var snd) ? snd != 0 : true;

        var builder = new ToastContentBuilder()
            .AddText(title);
        if (!string.IsNullOrWhiteSpace(body))
        {
            builder.AddText(body);
        }

        if (isHero && manifest.media?.hero is string hero && !string.IsNullOrWhiteSpace(hero))
        {
            var heroPath = Path.Combine(moduleRoot, hero);
            if (TryValidateHero(heroPath))
            {
                try { builder.AddHeroImage(new Uri(heroPath)); } catch { }
            }
            else
            {
                Logger.Write("WARN", $"Hero image skipped for '{manifest.id}' due to validation.");
            }
        }
        else if (manifest.media?.icon is string icon && !string.IsNullOrWhiteSpace(icon))
        {
            if (icon.StartsWith("preset:", StringComparison.OrdinalIgnoreCase))
            {
                var presetPath = GetPresetIconPath(icon);
                if (!string.IsNullOrEmpty(presetPath) && File.Exists(presetPath))
                {
                    try { builder.AddAppLogoOverride(new Uri(presetPath), ToastGenericAppLogoCrop.Default); } catch { }
                }
            }
            else
            {
                var iconPath = Path.Combine(moduleRoot, icon);
                if (TryValidateIcon(iconPath))
                {
                    try
                    {
                        builder.AddAppLogoOverride(new Uri(iconPath), ToastGenericAppLogoCrop.Default);
                    }
                    catch { }
                }
                else
                {
                    Logger.Write("WARN", $"Icon skipped for '{manifest.id}' due to validation.");
                }
            }
        }
        else if (!string.IsNullOrEmpty(_defaultLogoPath) && File.Exists(_defaultLogoPath) && !isHero)
        {
            try { builder.AddAppLogoOverride(new Uri(_defaultLogoPath), ToastGenericAppLogoCrop.Default); } catch { }
        }

        // OK button
        var tag = SafeTag(manifest.id);
        const string group = "module";

        builder.AddButton(new ToastButton()
            .SetContent("OK, I understand")
            .AddArgument("action", "complete")
            .AddArgument("module", manifest.id)
            .AddArgument("tag", tag)
            .AddArgument("group", group));

        // More info – opens a URL and should also complete the module
        if (manifest.media?.link is string link && !string.IsNullOrWhiteSpace(link))
        {
            // Ensure link has a scheme so Uri creation succeeds.
            var normalized = link;
            if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = "https://" + normalized;
            }

            if (Uri.TryCreate(normalized, UriKind.Absolute, out var linkUri))
            {
                builder.AddButton(new ToastButton()
                    .SetContent("More info")
                    .AddArgument("action", "link")
                    .AddArgument("module", manifest.id)
                    .AddArgument("url", linkUri.AbsoluteUri)
                    .AddArgument("tag", tag)
                    .AddArgument("group", group));
            }
        }

        try
        {
            var content = builder.GetToastContent();
            content.Scenario = hasReminder ? ToastScenario.Reminder : ToastScenario.Default;
            content.Audio = new ToastAudio { Silent = !soundEnabledSetting };

            var xml = content.GetXml();
            var toast = new ToastNotification(xml)
            {
                Tag = tag,
                Group = group
            };

            var expiresUtc = ParseDate(manifest.expires_utc);
            if (expiresUtc.HasValue) toast.ExpirationTime = expiresUtc.Value.ToLocalTime();

            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
            _ = _telemetry.SendAsync(manifest.id, "ToastShown", new Dictionary<string, object>
            {
                ["tag"] = toast.Tag ?? string.Empty,
                ["group"] = toast.Group ?? string.Empty
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Static activation handler invoked from activator class.
    public static void HandleActivation(string arguments)
    {
        try
        {
            var parsed = ActivationArgs.Parse(arguments);
            if (parsed == null || string.IsNullOrWhiteSpace(parsed.ModuleId))
            {
                return;
            }

            if (string.Equals(parsed.Action, "link", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(parsed.Url))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(parsed.Url) { UseShellExecute = true });
                    }
                    catch { }
                }

                _ = _telemetry.SendAsync(parsed.ModuleId, "ButtonMoreInfo", new Dictionary<string, object>
                {
                    ["url"] = parsed.Url ?? string.Empty,
                    ["tag"] = parsed.Tag ?? string.Empty,
                    ["group"] = parsed.Group ?? string.Empty,
                    ["args"] = parsed.Raw
                });
                CompleteModule(parsed.ModuleId);
                return;
            }

            if (string.Equals(parsed.Action, "complete", StringComparison.OrdinalIgnoreCase))
            {
                _ = _telemetry.SendAsync(parsed.ModuleId, "ButtonOk", new Dictionary<string, object>
                {
                    ["tag"] = parsed.Tag ?? string.Empty,
                    ["group"] = parsed.Group ?? string.Empty,
                    ["args"] = parsed.Raw
                });
                CompleteModule(parsed.ModuleId);
            }
        }
        catch { }
    }

    private static void CompleteModule(string moduleId)
    {
        var existing = ModuleStateStore.GetState(moduleId) ?? new ModuleState();
        var settings = CoreSettingsStore.GetSettings();
        var autoClear = settings.TryGetValue("AutoClearModules", out var ac) ? ac : 1;

        ModuleCompletion.Finish(
            moduleId,
            Path.Combine(ModulesRootDefault, moduleId),
            existing,
            DateTime.UtcNow,
            autoClear == 1);

        Logger.Write("INFO", $"Activation completed for module '{moduleId}'. AutoClear={autoClear}");
        _ = _telemetry.SendAsync(moduleId, "Completed");
    }

    private static int? ParseInt(string? value)
    {
        if (int.TryParse(value, out var i)) return i;
        return null;
    }

    private static string Clamp(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static string SafeTag(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId)) return Guid.NewGuid().ToString("N").Substring(0, 16);
        return moduleId.Length <= 64 ? moduleId : moduleId.Substring(0, 64);
    }

    private static bool TryValidateHero(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return false;

        try
        {
            var info = new FileInfo(path);
            if (info.Length > 1_024_000) return false; // 1 MB limit

            using var img = System.Drawing.Image.FromFile(path);
            var ratio = (double)img.Width / img.Height;
            if (ratio < 1.94 || ratio > 2.06) return false; // ~2:1 ±3%
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryValidateIcon(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;

        var extOk = path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase);
        if (!extOk) return false;

        try
        {
            var info = new FileInfo(path);
            if (info.Length > 512_000) return false; // 512 KB guardrail
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string EnsureDefaultLogo()
    {
        var preset = GetPresetIconPath("preset:info");
        if (!string.IsNullOrEmpty(preset))
        {
            return preset;
        }

        try
        {
            var path = Path.Combine(InstallRoot, "info.png");
            if (File.Exists(path)) return path;

            using var icon = (Icon)SystemIcons.Information.Clone();
            using var bmp = icon.ToBitmap();
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            return path;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string? GetPresetIconPath(string preset)
    {
        if (string.IsNullOrWhiteSpace(preset))
        {
            return null;
        }

        if (_presetIconCache.TryGetValue(preset, out var cached) && File.Exists(cached))
        {
            return cached;
        }

        var presetLower = preset.ToLowerInvariant();
        var fileName = presetLower switch
        {
            "preset:info" => "preset-info.png",
            "preset:warning" => "preset-warning.png",
            "preset:important" => "preset-important.png",
            "preset:question" => "preset-question.png",
            _ => null
        };

        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var destination = Path.Combine(InstallRoot, fileName);
        if (!File.Exists(destination))
        {
            Icon? sourceIcon = presetLower switch
            {
                "preset:info" => (Icon)SystemIcons.Information.Clone(),
                "preset:warning" => (Icon)SystemIcons.Warning.Clone(),
                "preset:important" => (Icon)SystemIcons.Error.Clone(),
                "preset:question" => (Icon)SystemIcons.Question.Clone(),
                _ => null
            };

            if (sourceIcon == null)
            {
                return null;
            }

            try
            {
                using var bmp = sourceIcon.ToBitmap();
                bmp.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                sourceIcon.Dispose();
                return null;
            }

            sourceIcon.Dispose();
        }

        _presetIconCache[preset] = destination;
        return destination;
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var d))
        {
            if (d.Kind == DateTimeKind.Unspecified)
            {
                d = DateTime.SpecifyKind(d, DateTimeKind.Utc);
            }
            return d.ToUniversalTime();
        }

        return null;
    }

    private static TelemetryClient CreateTelemetryClient()
    {
        // Priority: env vars > config file > local dev defaults.
        var url = Environment.GetEnvironmentVariable("WN_TELEMETRY_URL") ?? string.Empty;
        var key = Environment.GetEnvironmentVariable("WN_TELEMETRY_KEY") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            try
            {
                var cfgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CloudNotifier", "tray.config.json");
                if (File.Exists(cfgPath))
                {
                    var json = File.ReadAllText(cfgPath);
                    using var doc = JsonDocument.Parse(json);
                    if (string.IsNullOrWhiteSpace(url) && doc.RootElement.TryGetProperty("telemetryUrl", out var urlProp))
                    {
                        url = urlProp.GetString() ?? url;
                    }
                    if (string.IsNullOrWhiteSpace(key) && doc.RootElement.TryGetProperty("telemetryKey", out var keyProp))
                    {
                        key = keyProp.GetString() ?? key;
                    }
                }
            }
            catch
            {
                // best effort; ignore malformed config
            }
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            url = "http://localhost:5210/api/telemetry/events";
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            key = "dev-telemetry-key-change-me";
        }

        return new TelemetryClient(url, key);
    }
}
