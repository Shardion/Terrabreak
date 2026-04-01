using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZgePleaseWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout1Id",
                table: "DiscordPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout2Id",
                table: "DiscordPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout3Id",
                table: "DiscordPlayer");

            migrationBuilder.DropTable(
                name: "Loadout");

            migrationBuilder.DropIndex(
                name: "IX_DiscordPlayer_Loadout1Id",
                table: "DiscordPlayer");

            migrationBuilder.DropIndex(
                name: "IX_DiscordPlayer_Loadout2Id",
                table: "DiscordPlayer");

            migrationBuilder.DropIndex(
                name: "IX_DiscordPlayer_Loadout3Id",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1Id",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2Id",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3Id",
                table: "DiscordPlayer");

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Monster1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Monster2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Monster3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic4Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic5Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout1_Relic6Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Monster1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Monster2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Monster3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic4Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic5Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout2_Relic6Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Monster1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Monster2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Monster3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic1Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic2Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic3Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic4Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic5Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Loadout3_Relic6Identifier",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Loadout1_Monster1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Monster2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Monster3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic4Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic5Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout1_Relic6Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Monster1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Monster2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Monster3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic4Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic5Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout2_Relic6Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Monster1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Monster2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Monster3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic1Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic2Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic3Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic4Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic5Identifier",
                table: "DiscordPlayer");

            migrationBuilder.DropColumn(
                name: "Loadout3_Relic6Identifier",
                table: "DiscordPlayer");

            migrationBuilder.AddColumn<Guid>(
                name: "Loadout1Id",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Loadout2Id",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Loadout3Id",
                table: "DiscordPlayer",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Loadout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Monster1Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Monster2Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Monster3Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic1Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic2Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic3Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic4Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic5Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic6Identifier = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loadout", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_Loadout1Id",
                table: "DiscordPlayer",
                column: "Loadout1Id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_Loadout2Id",
                table: "DiscordPlayer",
                column: "Loadout2Id");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_Loadout3Id",
                table: "DiscordPlayer",
                column: "Loadout3Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout1Id",
                table: "DiscordPlayer",
                column: "Loadout1Id",
                principalTable: "Loadout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout2Id",
                table: "DiscordPlayer",
                column: "Loadout2Id",
                principalTable: "Loadout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordPlayer_Loadout_Loadout3Id",
                table: "DiscordPlayer",
                column: "Loadout3Id",
                principalTable: "Loadout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
