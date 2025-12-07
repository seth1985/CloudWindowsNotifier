using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowsNotifierCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModuleCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false)
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
                name: "ModuleDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ModuleId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CampaignId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    LinkUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConditionalScriptBody = table.Column<string>(type: "TEXT", nullable: true),
                    DynamicScriptBody = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ScheduleUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReminderHours = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IconFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    IconOriginalName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    DynamicMaxLength = table.Column<int>(type: "INTEGER", nullable: true),
                    DynamicTrimWhitespace = table.Column<bool>(type: "INTEGER", nullable: true),
                    DynamicFailIfEmpty = table.Column<bool>(type: "INTEGER", nullable: true),
                    DynamicFallbackMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CoreSettings_Enabled = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 1),
                    CoreSettings_PollingIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 300),
                    CoreSettings_AutoClearModules = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 1),
                    CoreSettings_SoundEnabled = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 1),
                    CoreSettings_ExitMenuVisible = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 0),
                    CoreSettings_StartStopMenuVisible = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 0),
                    CoreSettings_HeartbeatSeconds = table.Column<int>(type: "INTEGER", nullable: true, defaultValue: 15),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleDefinitions");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
