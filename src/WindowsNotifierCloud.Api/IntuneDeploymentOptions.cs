namespace WindowsNotifierCloud.Api;

public sealed class IntuneDeploymentOptions
{
    public bool Enabled { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GroupPrefix { get; set; } = "WN-";
    public string Publisher { get; set; } = "Windows Notifier";
}
