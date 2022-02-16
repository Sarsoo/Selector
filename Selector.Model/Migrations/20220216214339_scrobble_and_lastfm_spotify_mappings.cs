using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Selector.Model.Migrations
{
    public partial class scrobble_and_lastfm_spotify_mappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

            migrationBuilder.AlterColumn<string>(
                name: "LastFmUsername",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SaveScrobbles",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AlbumMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastfmAlbumName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    LastfmArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    SpotifyUri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastfmArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    SpotifyUri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scrobble",
                columns: table => new
                {
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TrackName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    AlbumName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    AlbumArtistName = table.Column<string>(type: "text", nullable: true),
                    ArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scrobble", x => new { x.UserId, x.Timestamp });
                    table.ForeignKey(
                        name: "FK_Scrobble_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastfmTrackName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    LastfmArtistName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    SpotifyUri = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackMapping", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "b91c880d-e280-4a17-a528-d34fdc35f291");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumMapping");

            migrationBuilder.DropTable(
                name: "ArtistMapping");

            migrationBuilder.DropTable(
                name: "Scrobble");

            migrationBuilder.DropTable(
                name: "TrackMapping");

            migrationBuilder.DropColumn(
                name: "SaveScrobbles",
                table: "AspNetUsers");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:CollationDefinition:case_insensitive", "en-u-ks-primary,en-u-ks-primary,icu,False");

            migrationBuilder.AlterColumn<string>(
                name: "LastFmUsername",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "0801d9f2-0f90-4ca7-bb85-eaa36046fc86");
        }
    }
}
