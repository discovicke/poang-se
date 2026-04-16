using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddGameModelModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CreatorOnly",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorSecret",
                table: "Games",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                table: "Games",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GameMode",
                table: "Games",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GameModeValue",
                table: "Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScoreIncrement",
                table: "Games",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "TeamBasedWinner",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClaimedByConnectionId",
                table: "GamePlayers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorOnly",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CreatorSecret",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameMode",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameModeValue",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ScoreIncrement",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TeamBasedWinner",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ClaimedByConnectionId",
                table: "GamePlayers");
        }
    }
}
