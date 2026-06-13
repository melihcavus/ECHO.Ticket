using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECHO.Ticket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueAndSeatEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColumnNumber",
                table: "Pledges",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowLabel",
                table: "Pledges",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    Columns = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Venues_VenueId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Events_VenueId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ColumnNumber",
                table: "Pledges");

            migrationBuilder.DropColumn(
                name: "RowLabel",
                table: "Pledges");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Events");
        }
    }
}
