using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Selector.Model.Migrations
{
    public partial class scrobble_int_id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scrobble_AspNetUsers_UserId",
                table: "Scrobble");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scrobble",
                table: "Scrobble");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Scrobble",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Scrobble",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scrobble",
                table: "Scrobble",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "765f8993-a743-496b-8c8a-e43f532ac862");

            migrationBuilder.CreateIndex(
                name: "IX_Scrobble_UserId",
                table: "Scrobble",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scrobble_AspNetUsers_UserId",
                table: "Scrobble",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scrobble_AspNetUsers_UserId",
                table: "Scrobble");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scrobble",
                table: "Scrobble");

            migrationBuilder.DropIndex(
                name: "IX_Scrobble_UserId",
                table: "Scrobble");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Scrobble");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Scrobble",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scrobble",
                table: "Scrobble",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "b91c880d-e280-4a17-a528-d34fdc35f291");

            migrationBuilder.AddForeignKey(
                name: "FK_Scrobble_AspNetUsers_UserId",
                table: "Scrobble",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
