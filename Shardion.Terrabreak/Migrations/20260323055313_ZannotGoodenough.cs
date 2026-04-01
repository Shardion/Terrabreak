using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ZannotGoodenough : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loadout",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Relic1Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic2Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic3Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic4Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic5Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Relic6Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Monster1Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Monster2Identifier = table.Column<string>(type: "TEXT", nullable: true),
                    Monster3Identifier = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loadout", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordPlayer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    EquippedLoadoutId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Loadout1Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Loadout2Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Loadout3Id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordPlayer_Loadout_EquippedLoadoutId",
                        column: x => x.EquippedLoadoutId,
                        principalTable: "Loadout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordPlayer_Loadout_Loadout1Id",
                        column: x => x.Loadout1Id,
                        principalTable: "Loadout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordPlayer_Loadout_Loadout2Id",
                        column: x => x.Loadout2Id,
                        principalTable: "Loadout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordPlayer_Loadout_Loadout3Id",
                        column: x => x.Loadout3Id,
                        principalTable: "Loadout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_DiscordUserId",
                table: "DiscordPlayer",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_EquippedLoadoutId",
                table: "DiscordPlayer",
                column: "EquippedLoadoutId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordPlayer");

            migrationBuilder.DropTable(
                name: "Loadout");
        }
    }
}
