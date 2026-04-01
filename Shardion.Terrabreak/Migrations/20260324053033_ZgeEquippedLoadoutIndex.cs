using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZgeEquippedLoadoutIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordPlayer_Loadout_EquippedLoadoutId",
                table: "DiscordPlayer");

            migrationBuilder.DropIndex(
                name: "IX_DiscordPlayer_EquippedLoadoutId",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "EquippedLoadoutId",
                table: "DiscordPlayer");

            migrationBuilder.AddColumn<uint>(
                name: "EquippedLoadout",
                table: "DiscordPlayer",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquippedLoadout",
                table: "DiscordPlayer");

            migrationBuilder.AddColumn<Guid>(
                name: "EquippedLoadoutId",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_EquippedLoadoutId",
                table: "DiscordPlayer",
                column: "EquippedLoadoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordPlayer_Loadout_EquippedLoadoutId",
                table: "DiscordPlayer",
                column: "EquippedLoadoutId",
                principalTable: "Loadout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
