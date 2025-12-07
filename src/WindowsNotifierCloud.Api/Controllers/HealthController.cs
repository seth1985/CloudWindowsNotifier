using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new { status = "ok", utc = DateTime.UtcNow });

    [HttpGet("ready")]
    [AllowAnonymous]
    public IActionResult Ready() => Ok(new { status = "ready", utc = DateTime.UtcNow });
}
