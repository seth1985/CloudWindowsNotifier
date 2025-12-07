using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowsNotifierCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TelemetryEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelemetryEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserPrincipalName = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false),
                    EventType = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AdditionalDataJson = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryEvents", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelemetryEvents");
        }
    }
}
