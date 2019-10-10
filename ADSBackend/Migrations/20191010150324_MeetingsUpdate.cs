using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class MeetingsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Meeting");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Meeting",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "PlannerId",
                table: "Meeting",
                newName: "OrganizerId");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Meeting",
                newName: "End");

            migrationBuilder.AddColumn<bool>(
                name: "AllDay",
                table: "Meeting",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Meeting",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Meeting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactId",
                table: "Meeting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "Meeting",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organizer",
                table: "Meeting",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllDay",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "Meeting");

            migrationBuilder.DropColumn(
                name: "Organizer",
                table: "Meeting");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Meeting",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "OrganizerId",
                table: "Meeting",
                newName: "PlannerId");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Meeting",
                newName: "EndTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Meeting",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
