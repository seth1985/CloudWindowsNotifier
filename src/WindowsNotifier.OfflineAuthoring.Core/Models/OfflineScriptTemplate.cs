namespace WindowsNotifier.OfflineAuthoring.Core.Models;

public sealed class OfflineScriptTemplate
{
    public string Name { get; set; } = string.Empty;
    public OfflineScriptTemplateType Type { get; set; }
    public string ScriptBody { get; set; } = string.Empty;

    public string DisplayName => $"{Name} ({Type})";
}
