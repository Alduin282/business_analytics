using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFileHashToImportSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "ImportSessions",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "ImportSessions");
        }
    }
}
