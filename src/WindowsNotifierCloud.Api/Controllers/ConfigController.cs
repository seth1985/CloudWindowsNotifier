using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly EnvironmentOptions _envOptions;
    private readonly ApplicationDbContext _db;

    public ConfigController(EnvironmentOptions envOptions, ApplicationDbContext db)
    {
        _envOptions = envOptions;
        _db = db;
    }

    [HttpGet("environment")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public IActionResult GetEnvironment()
    {
        var provider = _db.Database.ProviderName ?? "unknown";
        return Ok(new
        {
            mode = _envOptions.Mode,
            databaseProvider = provider
        });
    }
}
