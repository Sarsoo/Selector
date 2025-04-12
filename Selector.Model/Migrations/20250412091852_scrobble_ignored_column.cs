using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selector.Model.Migrations
{
    /// <inheritdoc />
    public partial class scrobble_ignored_column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ScrobbleIgnored",
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
                name: "ScrobbleIgnored",
                schema: "apple",
                table: "AppleMusicListen");
        }
    }
}
