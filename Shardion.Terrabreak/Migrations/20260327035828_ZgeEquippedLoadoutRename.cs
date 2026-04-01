using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZgeEquippedLoadoutRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquippedLoadout",
                table: "DiscordPlayer",
                newName: "EquippedLoadoutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquippedLoadoutId",
                table: "DiscordPlayer",
                newName: "EquippedLoadout");
        }
    }
}
