using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowsNotifierCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConditionalInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConditionalIntervalMinutes",
                table: "ModuleDefinitions",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionalIntervalMinutes",
                table: "ModuleDefinitions");
        }
    }
}
