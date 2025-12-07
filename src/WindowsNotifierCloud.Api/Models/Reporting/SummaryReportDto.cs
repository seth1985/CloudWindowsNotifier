namespace WindowsNotifierCloud.Api.Models.Reporting;

public record SummaryReportDto(
    int TotalEvents,
    int ToastShown,
    int ButtonOk,
    int ButtonMoreInfo,
    int Dismissed,
    int TimedOut,
    int ScriptError,
    int ConditionCheck,
    int Completed,
    DateTime RangeStartUtc,
    DateTime RangeEndUtc);
