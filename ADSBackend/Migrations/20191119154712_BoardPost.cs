using Microsoft.EntityFrameworkCore.Migrations;

namespace ADSBackend.Migrations
{
    public partial class BoardPost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardPost_Club_ClubId",
                table: "BoardPost");

            migrationBuilder.AlterColumn<int>(
                name: "ClubId",
                table: "BoardPost",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BoardPost_Club_ClubId",
                table: "BoardPost",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardPost_Club_ClubId",
                table: "BoardPost");

            migrationBuilder.AlterColumn<int>(
                name: "ClubId",
                table: "BoardPost",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_BoardPost_Club_ClubId",
                table: "BoardPost",
                column: "ClubId",
                principalTable: "Club",
                principalColumn: "ClubId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
