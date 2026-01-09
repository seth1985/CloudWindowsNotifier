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
            Role = PortalRole.Admin,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.PortalUsers.Add(adminUser);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DevDataSeeder: Seeded default Advanced user 'admin'.");

        // Seed a few sample PowerShell templates for the gallery
        if (!await _db.PowerShellTemplates.AnyAsync(cancellationToken))
        {
            var seedTemplates = new[]
            {
                new PowerShellTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = "Pending reboots",
                    Description = "Detects common reboot flags in registry and WMI.",
                    Category = "Maintenance",
                    ScriptBody = "if ((Get-ItemProperty -Path 'HKLM:SOFTWARE\\\\Microsoft\\\\Windows\\\\CurrentVersion\\\\WindowsUpdate\\\\Auto Update').RebootRequired -or (Get-CimInstance Win32_ComputerSystem).RebootPending) { exit 0 } else { exit 1 }",
                    Type = TemplateType.Conditional,
                    CreatedByUserId = adminUser.Id,
                    CreatedUtc = DateTime.UtcNow
                },
                new PowerShellTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = "List installed updates (recent 5)",
                    Description = "Shows last 5 installed hotfixes.",
                    Category = "Compliance",
                    ScriptBody = "Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 5 | ForEach-Object {\"$($_.HotFixID) $($_.InstalledOn)\"}",
                    Type = TemplateType.Dynamic,
                    CreatedByUserId = adminUser.Id,
                    CreatedUtc = DateTime.UtcNow
                },
                new PowerShellTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = "Disk free space (system drive)",
                    Description = "Reports free space on the system drive.",
                    Category = "Diagnostics",
                    ScriptBody = "$sys = Get-PSDrive -Name C; [PSCustomObject]@{ title = 'Disk space'; message = \"Free: $([math]::Round($sys.Free/1GB,2)) GB\" }",
                    Type = TemplateType.Dynamic,
                    CreatedByUserId = adminUser.Id,
                    CreatedUtc = DateTime.UtcNow
                }
            };

            _db.PowerShellTemplates.AddRange(seedTemplates);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("DevDataSeeder: Seeded sample PowerShell templates.");
        }
    }
}
