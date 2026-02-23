namespace WindowsNotifier.OfflineAuthoring.Core.Models;

public sealed class ModuleValidationResult
{
    public List<string> Errors { get; } = new();
    public List<ModuleValidationIssue> Issues { get; } = new();
    public bool IsValid => Errors.Count == 0;

    public void AddError(string field, string message)
    {
        Errors.Add(message);
        Issues.Add(new ModuleValidationIssue
        {
            Field = field,
            Message = message
        });
    }
}
