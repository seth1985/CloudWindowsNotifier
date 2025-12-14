using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Models.Modules;

public record ModuleDto(
    Guid Id,
    string DisplayName,
    string ModuleId,
    ModuleType Type,
    ModuleCategory Category,
    string? Description,
    int Version,
    bool IsPublished,
    string? IconFileName,
    string? IconOriginalName,
    string? HeroFileName,
    string? HeroOriginalName);

public static class ModuleMappings
{
    public static ModuleDto ToDto(this ModuleDefinition module) =>
        new(
            module.Id,
            module.DisplayName,
            module.ModuleId,
            module.Type,
            module.Category,
            module.Description,
            module.Version,
            module.IsPublished,
            module.IconFileName,
            module.IconOriginalName,
            module.HeroFileName,
            module.HeroOriginalName);
}
