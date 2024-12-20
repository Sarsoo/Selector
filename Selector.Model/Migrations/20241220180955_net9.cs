using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class net9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "4b4a37c7-cc65-485a-ac0e-d88ef6dede78");
        }
    }
}
