using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Models.Campaigns;

public record CampaignDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    int ModuleCount);

public static class CampaignMappings
{
    public static CampaignDto ToDto(this Campaign campaign) =>
        new(campaign.Id, campaign.Name, campaign.Description, campaign.CreatedAtUtc, campaign.Modules?.Count ?? 0);
}
