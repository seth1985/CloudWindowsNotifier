namespace WindowsNotifierCloud.Api.Auth;

public sealed class EntraOptions
{
    public string TenantId { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string ApiAudience { get; set; } = string.Empty;
    public string SpaClientId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

