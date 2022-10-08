using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Selector.Model.Migrations
{
    public partial class SpotifyHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpotifyListen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayedDuration = table.Column<int>(type: "integer", nullable: true),
                    TrackUri = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    TrackName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    AlbumName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    ArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotifyListen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotifyListen_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "4b4a37c7-cc65-485a-ac0e-d88ef6dede78");

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyListen_UserId",
                table: "SpotifyListen",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpotifyListen");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "ec454f56-2b26-4bd8-be8e-a7fd34981ac2");
        }
    }
}
