namespace WindowsNotifierCloud.Domain.Entities;

/// <summary>
/// Represents a user in the system with role-based access control.
/// </summary>
public class UserDefinition
{
    public int Id { get; set; }
    
    /// <summary>
    /// Display name from Azure Entra ID
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address (UPN) from Azure Entra ID
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User role: Standard, Advanced, or Admin
    /// </summary>
    public string Role { get; set; } = "Standard";
    
    /// <summary>
    /// Optional avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLogin { get; set; }
    
    /// <summary>
    /// When this user record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this user record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
