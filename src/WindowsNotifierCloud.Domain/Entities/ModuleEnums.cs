namespace WindowsNotifierCloud.Domain.Entities;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModuleType
{
    Standard,
    Conditional,
    Dynamic,
    CoreSettings,
    Hero
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModuleCategory
{
    GeneralInfo,
    Security,
    Compliance,
    Maintenance,
    Application,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplateType
{
    Conditional,
    Dynamic,
    Both
}
