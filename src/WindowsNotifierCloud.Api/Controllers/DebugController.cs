using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public DebugController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct)
    {
        var users = await _db.PortalUsers
            .Select(u => new { u.Id, u.LocalUsername, u.UserPrincipalName, u.Role })
            .ToListAsync(ct);
        return Ok(users);
    }
}
