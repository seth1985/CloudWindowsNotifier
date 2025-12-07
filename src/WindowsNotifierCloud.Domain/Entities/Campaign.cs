namespace WindowsNotifierCloud.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Guid CreatedByUserId { get; set; }
    public PortalUser? CreatedBy { get; set; }

    public ICollection<ModuleDefinition> Modules { get; set; } = new List<ModuleDefinition>();
}
