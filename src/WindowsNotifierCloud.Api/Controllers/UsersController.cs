using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UsersController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<UserDto>>> List(CancellationToken ct)
    {
        var users = await _db.Users
            .OrderBy(u => u.DisplayName)
            .ToListAsync(ct);

        return Ok(users.Select(u => new UserDto
        {
            Id = u.Id,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Role = u.Role,
            AvatarUrl = u.AvatarUrl,
            LastLogin = u.LastLogin
        }));
    }

    /// <summary>
    /// Get a specific user by ID (Admin only)
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> Get(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();

        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            LastLogin = user.LastLogin
        };
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// Simulates Azure Entra ID invitation
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        // Check if user already exists
        if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        var user = new UserDefinition
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            Role = request.Role ?? "Standard",
            AvatarUrl = request.AvatarUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            LastLogin = user.LastLogin
        });
    }

    /// <summary>
    /// Update user role (Admin only)
    /// </summary>
    [HttpPut("{id:int}/role")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();

        // Validate role
        if (request.Role != "Standard" && request.Role != "Advanced" && request.Role != "Admin")
        {
            return BadRequest(new { message = "Invalid role. Must be Standard, Advanced, or Admin." });
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Update user details (Admin only)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();

        // Check if email is being changed and if new email already exists
        if (user.Email != request.Email && await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
        {
            return Conflict(new { message = "User with this email already exists" });
        }

        user.DisplayName = request.DisplayName;
        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    /// Delete a user (Admin only)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> Delete(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}

// DTOs
public record UserDto
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public DateTime? LastLogin { get; init; }
}

public record CreateUserRequest
{
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Role { get; init; }
    public string? AvatarUrl { get; init; }
}

public record UpdateRoleRequest
{
    public string Role { get; init; } = string.Empty;
}

public record UpdateUserRequest
{
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
