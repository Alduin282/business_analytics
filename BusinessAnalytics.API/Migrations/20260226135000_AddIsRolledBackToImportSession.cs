using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRolledBackToImportSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRolledBack",
                table: "ImportSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRolledBack",
                table: "ImportSessions");
        }
    }
}
