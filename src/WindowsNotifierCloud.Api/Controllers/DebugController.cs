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

    [HttpPost("users/{id}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] string role, CancellationToken ct)
    {
        var user = await _db.PortalUsers.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();
        
        if (Enum.TryParse<Domain.Entities.PortalRole>(role, out var parsedRole))
        {
            user.Role = parsedRole;
            await _db.SaveChangesAsync(ct);
            return Ok(new { user.Id, user.LocalUsername, user.Role });
        }
        return BadRequest("Invalid role");
    }

}
