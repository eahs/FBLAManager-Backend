using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class MemberBio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profileImageSource",
                table: "Member",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "profileImageSource",
                table: "Member");
        }
    }
}
