namespace WindowsNotifierCloud.Domain.Entities;

public enum PortalRole
{
    Basic,
    Advanced
}

public class PortalUser
{
    public Guid Id { get; set; }
    public string? UserPrincipalName { get; set; } // For Entra-based users
    public string? DisplayName { get; set; }

    // For DevelopmentLocal only
    public string? LocalUsername { get; set; }
    public string? PasswordHash { get; set; }

    public PortalRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
