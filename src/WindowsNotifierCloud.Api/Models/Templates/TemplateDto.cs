using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Models.Templates;

public record TemplateDto(
    Guid Id,
    string Title,
    string? Description,
    string Category,
    TemplateType Type,
    string ScriptBody,
    DateTime CreatedUtc,
    string? CreatedBy);

public static class TemplateMappings
{
    public static TemplateDto ToDto(this PowerShellTemplate template) =>
        new(
            template.Id,
            template.Title,
            template.Description,
            template.Category,
            template.Type,
            template.ScriptBody,
            template.CreatedUtc,
            template.CreatedBy?.DisplayName);
}
