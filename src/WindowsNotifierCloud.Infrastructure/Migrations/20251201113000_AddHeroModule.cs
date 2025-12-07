using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowsNotifierCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroFileName",
                table: "ModuleDefinitions",
                type: "TEXT",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeroOriginalName",
                table: "ModuleDefinitions",
                type: "TEXT",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroFileName",
                table: "ModuleDefinitions");

            migrationBuilder.DropColumn(
                name: "HeroOriginalName",
                table: "ModuleDefinitions");
        }
    }
}
