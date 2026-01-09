using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Api;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/modules/{id:guid}/icon")]
public class ModuleIconController : ControllerBase
{
    private static readonly string[] AllowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".ico" };
    private readonly IModuleRepository _modules;
    private readonly StorageOptions _storage;

    public ModuleIconController(IModuleRepository modules, StorageOptions storage)
    {
        _modules = modules;
        _storage = storage;
    }

    [HttpPost]
    [Authorize(Policy = "BasicOrAdvanced")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadIcon(Guid id, IFormFile file, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest("Invalid file type. Allowed: .png, .jpg, .jpeg, .ico");

        if (string.IsNullOrWhiteSpace(_storage.Root))
            return StatusCode(500, "Storage root is not configured.");

        var targetDir = Path.Combine(_storage.Root, "module-assets", module.Id.ToString());
        Directory.CreateDirectory(targetDir);

        var safeName = Path.GetFileName(file.FileName);
        var targetPath = Path.Combine(targetDir, safeName);
        using (var stream = System.IO.File.Create(targetPath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var userId = Guid.Empty;
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(sub, out var parsed)) userId = parsed;

        module.UpdateIcon(safeName, file.FileName, userId);
        await _modules.SaveChangesAsync(ct);

        var previewUrl = Url.ActionLink(nameof(GetIcon), values: new { id });
        return Ok(new
        {
            success = true,
            fileName = safeName,
            previewUrl
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetIcon(Guid id, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null || string.IsNullOrWhiteSpace(module.IconFileName))
            return NotFound();

        var path = Path.Combine(_storage.Root, "module-assets", module.Id.ToString(), module.IconFileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        var ext = Path.GetExtension(path).ToLowerInvariant();
        var contentType = ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".ico" => "image/x-icon",
            _ => "application/octet-stream"
        };

        return PhysicalFile(path, contentType);
    }
}
