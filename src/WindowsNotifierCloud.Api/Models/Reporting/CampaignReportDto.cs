namespace WindowsNotifierCloud.Api.Models.Reporting;

public record CampaignReportDto(
    Guid CampaignId,
    string Name,
    int ModuleCount,
    int ToastShown,
    int ButtonOk,
    int ButtonMoreInfo,
    int Dismissed,
    int TimedOut,
    int Completed);
