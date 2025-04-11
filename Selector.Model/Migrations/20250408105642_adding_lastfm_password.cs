using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class adding_lastfm_password : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AddColumn<string>(
                name: "LastFmPassword",
                schema: "selector",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: false,
                defaultValue: "",
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldCollation: "case_insensitive");

            migrationBuilder.AddColumn<bool>(
                name: "IsScrobbled",
                schema: "apple",
                table: "AppleMusicListen",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFmPassword",
                schema: "selector",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsScrobbled",
                schema: "apple",
                table: "AppleMusicListen");

            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "spotify",
                table: "SpotifyListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "lastfm",
                table: "Scrobble",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "TrackName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "ArtistName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");

            migrationBuilder.AlterColumn<string>(
                name: "AlbumName",
                schema: "apple",
                table: "AppleMusicListen",
                type: "text",
                nullable: true,
                collation: "case_insensitive",
                oldClrType: typeof(string),
                oldType: "text",
                oldCollation: "case_insensitive");
        }
    }
}
