using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class service_specific_schemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "spotify");

            migrationBuilder.EnsureSchema(
                name: "apple");

            migrationBuilder.EnsureSchema(
                name: "selector");

            migrationBuilder.EnsureSchema(
                name: "lastfm");

            migrationBuilder.RenameTable(
                name: "Watcher",
                newName: "Watcher",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "TrackMapping",
                newName: "TrackMapping",
                newSchema: "spotify");

            migrationBuilder.RenameTable(
                name: "SpotifyListen",
                newName: "SpotifyListen",
                newSchema: "spotify");

            migrationBuilder.RenameTable(
                name: "Scrobble",
                newName: "Scrobble",
                newSchema: "lastfm");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "AspNetUserTokens",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "AspNetUsers",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "AspNetUserRoles",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "AspNetUserLogins",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "AspNetUserClaims",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "AspNetRoles",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "AspNetRoleClaims",
                newSchema: "selector");

            migrationBuilder.RenameTable(
                name: "ArtistMapping",
                newName: "ArtistMapping",
                newSchema: "spotify");

            migrationBuilder.RenameTable(
                name: "AppleMusicListen",
                newName: "AppleMusicListen",
                newSchema: "apple");

            migrationBuilder.RenameTable(
                name: "AlbumMapping",
                newName: "AlbumMapping",
                newSchema: "spotify");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Watcher",
                schema: "selector",
                newName: "Watcher");

            migrationBuilder.RenameTable(
                name: "TrackMapping",
                schema: "spotify",
                newName: "TrackMapping");

            migrationBuilder.RenameTable(
                name: "SpotifyListen",
                schema: "spotify",
                newName: "SpotifyListen");

            migrationBuilder.RenameTable(
                name: "Scrobble",
                schema: "lastfm",
                newName: "Scrobble");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                schema: "selector",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                schema: "selector",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                schema: "selector",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                schema: "selector",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                schema: "selector",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                schema: "selector",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                schema: "selector",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "ArtistMapping",
                schema: "spotify",
                newName: "ArtistMapping");

            migrationBuilder.RenameTable(
                name: "AppleMusicListen",
                schema: "apple",
                newName: "AppleMusicListen");

            migrationBuilder.RenameTable(
                name: "AlbumMapping",
                schema: "spotify",
                newName: "AlbumMapping");
        }
    }
}
