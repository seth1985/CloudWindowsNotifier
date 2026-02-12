using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WindowsNotifierCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortalUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserPrincipalName = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    LocalUsername = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelemetryEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionalDataJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Standard"),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Campaigns_PortalUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowerShellTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScriptBody = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerShellTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerShellTemplates_PortalUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModuleDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModuleId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    LinkUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConditionalScriptBody = table.Column<string>(type: "text", nullable: true),
                    ConditionalIntervalMinutes = table.Column<int>(type: "integer", nullable: true),
                    DynamicScriptBody = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduleUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReminderHours = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IconFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    IconOriginalName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    HeroFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    HeroOriginalName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    DynamicMaxLength = table.Column<int>(type: "integer", nullable: true),
                    DynamicTrimWhitespace = table.Column<bool>(type: "boolean", nullable: true),
                    DynamicFailIfEmpty = table.Column<bool>(type: "boolean", nullable: true),
                    DynamicFallbackMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoreSettings_Enabled = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    CoreSettings_PollingIntervalSeconds = table.Column<int>(type: "integer", nullable: true, defaultValue: 300),
                    CoreSettings_AutoClearModules = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    CoreSettings_SoundEnabled = table.Column<int>(type: "integer", nullable: true, defaultValue: 1),
                    CoreSettings_ExitMenuVisible = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CoreSettings_StartStopMenuVisible = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CoreSettings_HeartbeatSeconds = table.Column<int>(type: "integer", nullable: true, defaultValue: 15),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleDefinitions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ModuleDefinitions_PortalUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModuleDefinitions_PortalUsers_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "PortalUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedByUserId",
                table: "Campaigns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDefinitions_CampaignId",
                table: "ModuleDefinitions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDefinitions_CreatedByUserId",
                table: "ModuleDefinitions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDefinitions_LastModifiedByUserId",
                table: "ModuleDefinitions",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDefinitions_ModuleId",
                table: "ModuleDefinitions",
                column: "ModuleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerShellTemplates_CreatedByUserId",
                table: "PowerShellTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryEvents_EventType",
                table: "TelemetryEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryEvents_ModuleId",
                table: "TelemetryEvents",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryEvents_OccurredAtUtc",
                table: "TelemetryEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleDefinitions");

            migrationBuilder.DropTable(
                name: "PowerShellTemplates");

            migrationBuilder.DropTable(
                name: "TelemetryEvents");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "PortalUsers");
        }
    }
}
