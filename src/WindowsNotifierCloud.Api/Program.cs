using WindowsNotifierCloud.Api;
using WindowsNotifierCloud.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using WindowsNotifierCloud.Api.Auth;
using WindowsNotifierCloud.Domain.Interfaces;
using WindowsNotifierCloud.Infrastructure.Repositories;
using WindowsNotifierCloud.Api.Seed;
using System.IdentityModel.Tokens.Jwt;
using WindowsNotifierCloud.Api.Services;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Prevent mapping of short claim names to long URIs
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
    storageOptions.Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Windows Notifier", "ApiStorage");
}
builder.Services.AddSingleton(storageOptions);
var telemetrySection = builder.Configuration.GetSection("Telemetry");
var telemetryOptions = telemetrySection.Get<TelemetryOptions>() ?? new TelemetryOptions();
builder.Services.AddSingleton(telemetryOptions);
var authSection = builder.Configuration.GetSection("Authentication");
var authOptions = authSection.Get<AuthenticationOptions>() ?? new AuthenticationOptions();
builder.Services.AddSingleton(authOptions);
var entraSection = builder.Configuration.GetSection("Entra");
var entraOptions = entraSection.Get<EntraOptions>() ?? new EntraOptions();
builder.Services.AddSingleton(entraOptions);

// Bind JWT options
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);
var jwtOpts = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

// Configure EF Core (PostgreSQL for all environments)
var configuredConnectionString = builder.Configuration.GetConnectionString("Default");
var defaultPgConnectionString = "Host=localhost;Port=5432;Database=windows_notifier_cloud;Username=postgres;Password=postgres";
var connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
    ? defaultPgConnectionString
    : Environment.ExpandEnvironmentVariables(configuredConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services (will expand in later tasks)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
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
    options.DefaultAuthenticateScheme = "DynamicAuth";
    options.DefaultChallengeScheme = "DynamicAuth";
})
.AddPolicyScheme("DynamicAuth", "Select auth scheme from Authentication:Provider", options =>
{
    options.ForwardDefaultSelector = _ =>
    {
        return string.Equals(authOptions.Provider, "Entra", StringComparison.OrdinalIgnoreCase)
            ? "EntraJwt"
            : "LocalJwt";
    };
})
.AddJwtBearer("LocalJwt", options =>
{
    options.MapInboundClaims = false;
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
        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
    };
})
.AddJwtBearer("EntraJwt", options =>
{
    var authority = string.IsNullOrWhiteSpace(entraOptions.Authority)
        ? $"https://login.microsoftonline.com/{entraOptions.TenantId}/v2.0"
        : entraOptions.Authority;
    options.Authority = authority;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = entraOptions.ApiAudience,
        RoleClaimType = "roles",
        NameClaimType = "name"
    };
    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicOrAdvanced", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AdvancedOnly", policy =>
        policy.RequireRole("Advanced"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
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

// Ensure database exists for configured PostgreSQL target.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Seed dev-local defaults (e.g., initial Advanced user)
if (string.Equals(environmentMode, "DevelopmentLocal", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    await seeder.SeedAsync();
}

app.Run();
// Log success
Console.WriteLine("API is running and ready.");
