using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Teams_TeamId",
                table: "GamePlayers");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Teams_TeamId",
                table: "GamePlayers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Teams_TeamId",
                table: "GamePlayers");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Teams_TeamId",
                table: "GamePlayers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
