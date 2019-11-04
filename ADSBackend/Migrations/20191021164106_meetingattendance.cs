using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class meetingattendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Member_Meeting_MeetingId",
                table: "Member");

            migrationBuilder.DropIndex(
                name: "IX_Member_MeetingId",
                table: "Member");

            migrationBuilder.DropColumn(
                name: "MeetingId",
                table: "Member");

            migrationBuilder.CreateTable(
                name: "MeetingAttendees",
                columns: table => new
                {
                    MemberId = table.Column<int>(nullable: false),
                    MeetingId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingAttendees", x => new { x.MeetingId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_Meeting_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meeting",
                        principalColumn: "MeetingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingAttendees_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingAttendees_MemberId",
                table: "MeetingAttendees",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingAttendees");

            migrationBuilder.AddColumn<int>(
                name: "MeetingId",
                table: "Member",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Member_MeetingId",
                table: "Member",
                column: "MeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Member_Meeting_MeetingId",
                table: "Member",
                column: "MeetingId",
                principalTable: "Meeting",
                principalColumn: "MeetingId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
