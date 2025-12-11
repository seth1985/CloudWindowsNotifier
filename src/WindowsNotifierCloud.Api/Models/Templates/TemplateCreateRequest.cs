using System.ComponentModel.DataAnnotations;
using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Api.Models.Templates;

public class TemplateCreateRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public TemplateType Type { get; set; } = TemplateType.Conditional;

    [Required]
    public string ScriptBody { get; set; } = string.Empty;
}
