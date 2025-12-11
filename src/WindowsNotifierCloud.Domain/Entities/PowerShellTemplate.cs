namespace WindowsNotifierCloud.Domain.Entities;

public class PowerShellTemplate
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ScriptBody { get; set; } = string.Empty;
    public TemplateType Type { get; set; }
    public Guid CreatedByUserId { get; set; }
    public PortalUser? CreatedBy { get; set; }
    public DateTime CreatedUtc { get; set; }
}
