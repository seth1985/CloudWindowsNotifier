using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Api.Auth;
using WindowsNotifierCloud.Domain.Entities;
using WindowsNotifierCloud.Infrastructure.Persistence;

namespace WindowsNotifierCloud.Api.Seed;

/// <summary>
/// Seeds a default Advanced user for DevelopmentLocal runs so the portal can be accessed immediately.
/// </summary>
public sealed class DevDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DevDataSeeder> _logger;

    public DevDataSeeder(ApplicationDbContext db, ILogger<DevDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);

        var anyUsers = await _db.PortalUsers.AnyAsync(cancellationToken);
        if (anyUsers)
        {
            _logger.LogInformation("DevDataSeeder: Users already present; skipping seed.");
            return;
        }

        var adminUser = new PortalUser
        {
            Id = Guid.NewGuid(),
            LocalUsername = "admin",
            DisplayName = "Admin (DevLocal)",
            PasswordHash = PasswordHasher.HashPassword("P@ssw0rd!"),
            Role = PortalRole.Advanced,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.PortalUsers.Add(adminUser);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DevDataSeeder: Seeded default Advanced user 'admin'.");
    }
}
