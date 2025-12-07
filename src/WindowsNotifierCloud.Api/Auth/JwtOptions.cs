namespace WindowsNotifierCloud.Api.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = "WindowsNotifierCloud";
    public string Audience { get; set; } = "WindowsNotifierCloud";
    public string Key { get; set; } = "change-this-key";
    public int ExpiryMinutes { get; set; } = 120;
}
