using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZgeEquippedLoadoutIdRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquippedLoadoutId",
                table: "DiscordPlayer",
                newName: "EquippedLoadoutIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EquippedLoadoutIndex",
                table: "DiscordPlayer",
                newName: "EquippedLoadoutId");
        }
    }
}
