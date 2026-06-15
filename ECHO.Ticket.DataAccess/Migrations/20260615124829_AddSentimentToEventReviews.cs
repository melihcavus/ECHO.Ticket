using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECHO.Ticket.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSentimentToEventReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "EventReviews",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "SentimentScore",
                table: "EventReviews",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "EventReviews");

            migrationBuilder.DropColumn(
                name: "SentimentScore",
                table: "EventReviews");
        }
    }
}
