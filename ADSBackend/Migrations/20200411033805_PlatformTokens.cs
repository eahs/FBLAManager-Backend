using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class PlatformTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppleToken",
                table: "Session",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleToken",
                table: "Session",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppleToken",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "GoogleToken",
                table: "Session");
        }
    }
}
