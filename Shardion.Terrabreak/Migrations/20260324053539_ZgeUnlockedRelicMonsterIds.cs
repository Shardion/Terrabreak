using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZgeUnlockedRelicMonsterIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnlockedMonsterIdentifiers",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "UnlockedRelicIdentifiers",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockedMonsterIdentifiers",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "UnlockedRelicIdentifiers",
                table: "DiscordPlayer");
        }
    }
}
