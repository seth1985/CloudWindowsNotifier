namespace WindowsNotifierTray;

public sealed class ModuleManifest
{
    public string id { get; set; } = string.Empty;
    public string? title { get; set; }
    public string? message { get; set; }
    public ModuleMedia? media { get; set; }
    public ModuleBehavior? behavior { get; set; }
    public string? type { get; set; }
    public ModuleManifestCoreSettings? core_settings { get; set; }
    public ModuleDynamic? dynamic { get; set; }
    public string? schedule_utc { get; set; }
    public string? expires_utc { get; set; }
    public string? created_utc { get; set; }
    public string? script { get; set; }          // dynamic/conditional
}

public sealed class ModuleMedia
{
    public string? icon { get; set; }
    public string? hero { get; set; }
    public string? link { get; set; }
}

public sealed class ModuleBehavior
{
    public int? reminder_hours { get; set; }
    public int? conditional_interval_minutes { get; set; }
    public string? conditional_script { get; set; }
}

public sealed class ModuleDynamic
{
    public string? script { get; set; }
    public int? max_length { get; set; }
    public ModuleDynamicOptions? options { get; set; }
}

public sealed class ModuleDynamicOptions
{
    public bool? trim_whitespace { get; set; }
    public bool? fail_if_empty { get; set; }
    public string? fallback_message { get; set; }
}

public sealed class ModuleManifestCoreSettings
{
    public int? auto_clear_modules { get; set; }
    public int? heartbeat_seconds { get; set; }
    public int? polling_interval_seconds { get; set; }
    public int? sound_enabled { get; set; }
    public int? enabled { get; set; }
    public int? exit_menu_visible { get; set; }
    public int? start_stop_menu_visible { get; set; }
}
