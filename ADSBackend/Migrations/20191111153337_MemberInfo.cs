using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class MemberInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Member",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecruitedBy",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Member",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Member",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "RecruitedBy",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Member");
        }
    }
}
