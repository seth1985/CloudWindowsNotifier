namespace WindowsNotifierCloud.Api;

public sealed class StorageOptions
{
    public string Root { get; set; } = string.Empty;
    public string? DevCoreModulesRoot { get; set; }
    public StorageRetentionOptions Retention { get; set; } = new();
}

public sealed class StorageRetentionOptions
{
    public int RetainExportVersions { get; set; } = 3;
    public int MaxExportAgeDays { get; set; } = 30;
    public int MaxZipAgeDays { get; set; } = 30;
    public bool PruneOrphans { get; set; } = true;
}
