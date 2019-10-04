using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class ClubMembersUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberClubs");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Member",
                newName: "MemberId");

            migrationBuilder.CreateTable(
                name: "ClubMember",
                columns: table => new
                {
                    MemberId = table.Column<int>(nullable: false),
                    ClubId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMember", x => new { x.ClubId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_ClubMember_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubMember_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMember_MemberId",
                table: "ClubMember",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubMember");

            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Member",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "MemberClubs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClubId = table.Column<int>(nullable: false),
                    MemberId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberClubs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberClubs_Club_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Club",
                        principalColumn: "ClubId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberClubs_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberClubs_ClubId",
                table: "MemberClubs",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberClubs_MemberId",
                table: "MemberClubs",
                column: "MemberId");
        }
    }
}
