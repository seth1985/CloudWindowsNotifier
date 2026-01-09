using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WindowsNotifierCloud.Domain.Interfaces;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/modules/{id:guid}/hero")]
public class ModuleHeroController : ControllerBase
{
    private readonly IModuleRepository _modules;
    private readonly StorageOptions _storage;

    public ModuleHeroController(IModuleRepository modules, StorageOptions storage)
    {
        _modules = modules;
        _storage = storage;
    }

    [HttpPost]
    [Authorize(Policy = "BasicOrAdvanced")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadHero(Guid id, IFormFile file, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null) return NotFound();

        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!string.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Hero image must be a .png file.");

        if (file.Length > 1024 * 1024)
            return BadRequest("Hero image exceeds maximum size of 1024 KB.");

        if (string.IsNullOrWhiteSpace(_storage.Root))
            return StatusCode(500, "Storage root is not configured.");

        using var mem = new MemoryStream();
        await file.CopyToAsync(mem, ct);
        mem.Position = 0;

        try
        {
            using var img = Image.FromStream(mem, useEmbeddedColorManagement: false, validateImageData: true);
            var w = img.Width;
            var h = img.Height;
            const double aspectTarget = 2.0;
            var aspect = (double)w / h;
            var aspectDiff = Math.Abs(aspect - aspectTarget) / aspectTarget;
            if (w < 364 || h < 180 || w > 728 || h > 360 || aspectDiff > 0.03)
            {
                return BadRequest($"Hero image must be between 364x180 and 728x360 with ~2:1 aspect (found {w}x{h}).");
            }
        }
        catch
        {
            return BadRequest("Invalid image file.");
        }

        // reset stream for saving
        mem.Position = 0;

        var targetDir = Path.Combine(_storage.Root, "module-assets", module.Id.ToString());
        Directory.CreateDirectory(targetDir);

        var targetPath = Path.Combine(targetDir, "hero.png");
        await System.IO.File.WriteAllBytesAsync(targetPath, mem.ToArray(), ct);

        var userId = Guid.Empty;
        var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(sub, out var parsed)) userId = parsed;

        module.UpdateHero("hero.png", file.FileName, userId);
        await _modules.SaveChangesAsync(ct);

        var previewUrl = Url.ActionLink(nameof(GetHero), values: new { id });
        return Ok(new
        {
            success = true,
            fileName = "hero.png",
            previewUrl
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetHero(Guid id, CancellationToken ct)
    {
        var module = await _modules.GetAsync(id, ct);
        if (module == null || string.IsNullOrWhiteSpace(module.HeroFileName))
            return NotFound();

        var path = Path.Combine(_storage.Root, "module-assets", module.Id.ToString(), module.HeroFileName);
        if (!System.IO.File.Exists(path))
            return NotFound();

        return PhysicalFile(path, "image/png");
    }
}
