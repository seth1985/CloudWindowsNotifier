namespace WindowsNotifierCloud.Api.Models.Campaigns;

public class CampaignCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
