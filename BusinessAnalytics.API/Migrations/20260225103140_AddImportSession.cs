using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessAnalytics.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImportSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImportSessionId",
                table: "Orders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OrdersCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemsCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ImportSessionId",
                table: "Orders",
                column: "ImportSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ImportSessions_ImportSessionId",
                table: "Orders",
                column: "ImportSessionId",
                principalTable: "ImportSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ImportSessions_ImportSessionId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ImportSessions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ImportSessionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ImportSessionId",
                table: "Orders");
        }
    }
}
