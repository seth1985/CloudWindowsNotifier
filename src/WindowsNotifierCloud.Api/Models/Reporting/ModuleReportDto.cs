namespace WindowsNotifierCloud.Api.Models.Reporting;

public record ModuleReportDto(
    string ModuleId,
    string? DisplayName,
    string? Type,
    string? Category,
    int ToastShown,
    int ButtonOk,
    int ButtonMoreInfo,
    int Dismissed,
    int TimedOut,
    int ScriptError,
    int ConditionCheck,
    int Completed,
    DateTime? FirstEventUtc,
    DateTime? LastEventUtc);
