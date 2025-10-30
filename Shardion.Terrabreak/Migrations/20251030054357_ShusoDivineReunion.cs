using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shardion.Terrabreak.Migrations
{
    /// <inheritdoc />
    public partial class ShusoDivineReunion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordPlayer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Credits = table.Column<int>(type: "INTEGER", nullable: false),
                    Ribbons = table.Column<int>(type: "INTEGER", nullable: false),
                    WeaponId = table.Column<string>(type: "TEXT", nullable: false),
                    ShieldId = table.Column<string>(type: "TEXT", nullable: false),
                    HealId = table.Column<string>(type: "TEXT", nullable: true),
                    CureId = table.Column<string>(type: "TEXT", nullable: true),
                    StrongestEnemy_EnemyId = table.Column<string>(type: "TEXT", nullable: true),
                    StrongestEnemy_SourceChannelId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordPlayer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SdrChannel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    TakenOver = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaptorId = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SdrChannel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SdrServer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ServerId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    PassagesUnlocked = table.Column<string>(type: "TEXT", nullable: false),
                    TakenOver = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SdrServer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordPlayer_UserId",
                table: "DiscordPlayer",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SdrChannel_ChannelId",
                table: "SdrChannel",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SdrChannel_ServerId",
                table: "SdrChannel",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_SdrServer_ServerId",
                table: "SdrServer",
                column: "ServerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordPlayer");

            migrationBuilder.DropTable(
                name: "SdrChannel");

            migrationBuilder.DropTable(
                name: "SdrServer");
        }
    }
}
