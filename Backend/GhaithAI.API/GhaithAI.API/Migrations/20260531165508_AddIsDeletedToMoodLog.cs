using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToMoodLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MoodLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MoodLogs");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ChatSessions");
        }
    }
}
