using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class add_apple_listen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppleMusicListen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackId = table.Column<string>(type: "text", nullable: true),
                    Isrc = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    TrackName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    AlbumName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    ArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppleMusicListen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppleMusicListen_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppleMusicListen_UserId",
                table: "AppleMusicListen",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppleMusicListen");
        }
    }
}
