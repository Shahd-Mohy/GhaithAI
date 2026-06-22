using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.Migrations
{
    /// <inheritdoc />
    public partial class AddReportRichFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaintDuration",
                table: "SessionReports",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaintPrimary",
                table: "SessionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifferentialConsiderations",
                table: "SessionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskNarrative",
                table: "SessionReports",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuicidalIdeationDetails",
                table: "SessionReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChiefComplaintDuration",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "ChiefComplaintPrimary",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "DifferentialConsiderations",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "RiskNarrative",
                table: "SessionReports");

            migrationBuilder.DropColumn(
                name: "SuicidalIdeationDetails",
                table: "SessionReports");
        }
    }
}
