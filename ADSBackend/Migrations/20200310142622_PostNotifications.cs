using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class PostNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Member",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BoardPost",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WriteTime",
                table: "BoardPost",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BoardPost");

            migrationBuilder.DropColumn(
                name: "WriteTime",
                table: "BoardPost");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Member",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
