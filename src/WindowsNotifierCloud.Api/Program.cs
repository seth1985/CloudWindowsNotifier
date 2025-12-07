using WindowsNotifierCloud.Api;
using WindowsNotifierCloud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WindowsNotifierCloud.Api.Auth;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Repositories;
using WindowsNotifierCloud.Api.Seed;
using System.IdentityModel.Tokens.Jwt;
using WindowsNotifierCloud.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind environment mode (DevelopmentLocal or ProductionCloud)
var envSection = builder.Configuration.GetSection("Environment");
var environmentMode = envSection.GetValue<string>("Mode") ?? "DevelopmentLocal";
builder.Services.AddSingleton(new EnvironmentOptions { Mode = environmentMode });
var storageSection = builder.Configuration.GetSection("Storage");
var storageOptions = storageSection.Get<StorageOptions>() ?? new StorageOptions();
storageOptions.Root = Environment.ExpandEnvironmentVariables(storageOptions.Root ?? string.Empty);
storageOptions.DevCoreModulesRoot = string.IsNullOrWhiteSpace(storageOptions.DevCoreModulesRoot)
    ? null
    : Environment.ExpandEnvironmentVariables(storageOptions.DevCoreModulesRoot);
if (string.IsNullOrWhiteSpace(storageOptions.Root))
{
    storageOptions.Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CloudNotifier", "ApiStorage");
}
builder.Services.AddSingleton(storageOptions);
var telemetrySection = builder.Configuration.GetSection("Telemetry");
var telemetryOptions = telemetrySection.Get<TelemetryOptions>() ?? new TelemetryOptions();
builder.Services.AddSingleton(telemetryOptions);

// Bind JWT options
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOpts = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

// Configure EF Core (SQLite for DevelopmentLocal; placeholder for ProductionCloud)
if (string.Equals(environmentMode, "DevelopmentLocal", StringComparison.OrdinalIgnoreCase))
{
    var dbPath = Path.Combine(AppContext.BaseDirectory, "App_Data");
    Directory.CreateDirectory(dbPath);
    var sqlitePath = Path.Combine(dbPath, "wncloud.db");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}
else
{
    // TODO: configure SQL Server for ProductionCloud
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=:memory:")); // placeholder to keep DI happy
}

// Add services (will expand in later tasks)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:5174",
            "http://127.0.0.1:5174")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
builder.Services.AddScoped<IPortalUserRepository, PortalUserRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddScoped<DevDataSeeder>();
builder.Services.AddSingleton<ManifestBuilder>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<StorageService>();
builder.Services.AddSingleton<StorageCleanupService>();
builder.Services.AddHostedService<StorageCleanupHostedService>();

// Auth (JWT for DevelopmentLocal local login)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOpts.Issuer,
        ValidAudience = jwtOpts.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.Key)),
        ClockSkew = TimeSpan.FromMinutes(1),
        RoleClaimType = "role",
        NameClaimType = JwtRegisteredClaimNames.Sub
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicOrAdvanced", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AdvancedOnly", policy =>
        policy.RequireClaim("role", "Advanced"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowDevFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed dev-local defaults (e.g., initial Advanced user)
if (string.Equals(environmentMode, "DevelopmentLocal", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
