using Microsoft.EntityFrameworkCore;
using WindowsNotifierCloud.Domain.Entities;

namespace WindowsNotifierCloud.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<ModuleDefinition> ModuleDefinitions => Set<ModuleDefinition>();
    public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PortalUser>(entity =>
        {
            entity.ToTable("PortalUsers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Role).HasConversion<int>();
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(c => c.CreatedBy)
                  .WithMany()
                  .HasForeignKey(c => c.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModuleDefinition>(entity =>
        {
            entity.ToTable("ModuleDefinitions");
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ModuleId).IsUnique();

            entity.Property(m => m.DisplayName).IsRequired().HasMaxLength(200);
            entity.Property(m => m.ModuleId).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Title).HasMaxLength(200);
            entity.Property(m => m.Message).HasMaxLength(1024);
            entity.Property(m => m.LinkUrl).HasMaxLength(500);
            entity.Property(m => m.Description).HasMaxLength(2000);
            entity.Property(m => m.IconFileName).HasMaxLength(260);
            entity.Property(m => m.IconOriginalName).HasMaxLength(260);
            entity.Property(m => m.HeroFileName).HasMaxLength(260);
            entity.Property(m => m.HeroOriginalName).HasMaxLength(260);
            entity.Property(m => m.ReminderHours).HasMaxLength(200);
            entity.Property(m => m.DynamicFallbackMessage).HasMaxLength(500);
            entity.Property(m => m.Type).HasConversion<int>();
            entity.Property(m => m.Category).HasConversion<int>();

            entity.HasOne(m => m.Campaign)
                  .WithMany(c => c.Modules)
                  .HasForeignKey(m => m.CampaignId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.CreatedBy)
                  .WithMany()
                  .HasForeignKey(m => m.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.LastModifiedBy)
                  .WithMany()
                  .HasForeignKey(m => m.LastModifiedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.OwnsOne(m => m.CoreSettings, owned =>
            {
                owned.Property(c => c.Enabled).HasDefaultValue(1);
                owned.Property(c => c.PollingIntervalSeconds).HasDefaultValue(300);
                owned.Property(c => c.AutoClearModules).HasDefaultValue(1);
                owned.Property(c => c.SoundEnabled).HasDefaultValue(1);
                owned.Property(c => c.ExitMenuVisible).HasDefaultValue(0);
                owned.Property(c => c.StartStopMenuVisible).HasDefaultValue(0);
                owned.Property(c => c.HeartbeatSeconds).HasDefaultValue(15);
            });
        });

        modelBuilder.Entity<TelemetryEvent>(entity =>
        {
            entity.ToTable("TelemetryEvents");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.ModuleId).IsRequired().HasMaxLength(200);
            entity.Property(t => t.DeviceId).IsRequired().HasMaxLength(200);
            entity.Property(t => t.UserPrincipalName).IsRequired().HasMaxLength(320);
            entity.Property(t => t.EventType).HasConversion<int>();
            entity.Property(t => t.OccurredAtUtc).IsRequired();
            entity.Property(t => t.AdditionalDataJson).HasColumnType("TEXT");
            entity.HasIndex(t => t.ModuleId);
            entity.HasIndex(t => t.EventType);
            entity.HasIndex(t => t.OccurredAtUtc);
        });
    }
}
