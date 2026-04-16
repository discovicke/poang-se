using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Games"" ADD COLUMN IF NOT EXISTS ""IsPrivate"" boolean NOT NULL DEFAULT FALSE;
                ALTER TABLE ""Games"" ADD COLUMN IF NOT EXISTS ""PasswordHash"" text;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Games");
        }
    }
}
