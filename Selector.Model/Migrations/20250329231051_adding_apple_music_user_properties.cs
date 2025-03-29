using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class adding_apple_music_user_properties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppleMusicKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppleMusicLastRefresh",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "AppleMusicLinked",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppleMusicKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AppleMusicLastRefresh",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AppleMusicLinked",
                table: "AspNetUsers");
        }
    }
}
