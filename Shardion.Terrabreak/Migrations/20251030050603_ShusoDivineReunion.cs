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
            migrationBuilder.AddColumn<bool>(
                name: "TakenOver",
                table: "SdrServer",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TakenOver",
                table: "SdrServer");
        }
    }
}
