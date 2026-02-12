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
    private readonly WindowsNotifierCloud.Api.Auth.AuthenticationOptions _authOptions;
    private readonly WindowsNotifierCloud.Api.Auth.EntraOptions _entraOptions;

    public ConfigController(
        EnvironmentOptions envOptions,
        ApplicationDbContext db,
        WindowsNotifierCloud.Api.Auth.AuthenticationOptions authOptions,
        WindowsNotifierCloud.Api.Auth.EntraOptions entraOptions)
    {
        _envOptions = envOptions;
        _db = db;
        _authOptions = authOptions;
        _entraOptions = entraOptions;
    }

    [HttpGet("environment")]
    [Authorize(Policy = "BasicOrAdvanced")]
    public IActionResult GetEnvironment()
    {
        var provider = _db.Database.ProviderName ?? "unknown";
        return Ok(new
        {
            mode = _envOptions.Mode,
            databaseProvider = provider,
            authProvider = _authOptions.Provider
        });
    }

    [HttpGet("auth")]
    [AllowAnonymous]
    public IActionResult GetAuth()
    {
        var provider = string.Equals(_authOptions.Provider, "Entra", StringComparison.OrdinalIgnoreCase)
            ? "Entra"
            : "Local";
        return Ok(new
        {
            provider,
            entra = new
            {
                tenantId = _entraOptions.TenantId,
                authority = _entraOptions.Authority,
                spaClientId = _entraOptions.SpaClientId,
                scope = _entraOptions.Scope
            }
        });
    }
}
