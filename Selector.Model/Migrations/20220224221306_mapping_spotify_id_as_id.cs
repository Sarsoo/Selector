using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Selector.Model.Migrations
{
    public partial class mapping_spotify_id_as_id : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackMapping",
                table: "TrackMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistMapping",
                table: "ArtistMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlbumMapping",
                table: "AlbumMapping");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TrackMapping");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ArtistMapping");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AlbumMapping");

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "TrackMapping",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "ArtistMapping",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "AlbumMapping",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackMapping",
                table: "TrackMapping",
                column: "SpotifyUri");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistMapping",
                table: "ArtistMapping",
                column: "SpotifyUri");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlbumMapping",
                table: "AlbumMapping",
                column: "SpotifyUri");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "ec454f56-2b26-4bd8-be8e-a7fd34981ac2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TrackMapping",
                table: "TrackMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistMapping",
                table: "ArtistMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlbumMapping",
                table: "AlbumMapping");

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "TrackMapping",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "TrackMapping",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "ArtistMapping",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ArtistMapping",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "SpotifyUri",
                table: "AlbumMapping",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AlbumMapping",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TrackMapping",
                table: "TrackMapping",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistMapping",
                table: "ArtistMapping",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlbumMapping",
                table: "AlbumMapping",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "00c64c0a-3387-4933-9575-83443fa9092b",
                column: "ConcurrencyStamp",
                value: "765f8993-a743-496b-8c8a-e43f532ac862");
        }
    }
}
