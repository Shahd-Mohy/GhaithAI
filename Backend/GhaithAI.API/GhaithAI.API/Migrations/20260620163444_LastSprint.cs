using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.Migrations
{
    /// <inheritdoc />
    public partial class LastSprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "InProgress"),
                    SessionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VideoRoomId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VideoRoomUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalSessions_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalSessions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalSessions_DoctorsProfiles_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "DoctorsProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    AiDraftJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinalContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalReports_ClinicalSessions_ClinicalSessionId",
                        column: x => x.ClinicalSessionId,
                        principalTable: "ClinicalSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Quick"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionNotes_ClinicalSessions_ClinicalSessionId",
                        column: x => x.ClinicalSessionId,
                        principalTable: "ClinicalSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionTranscripts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Speaker = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartMs = table.Column<int>(type: "int", nullable: false),
                    EndMs = table.Column<int>(type: "int", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTranscripts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTranscripts_ClinicalSessions_ClinicalSessionId",
                        column: x => x.ClinicalSessionId,
                        principalTable: "ClinicalSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalReportHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalAiContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalChangesCount = table.Column<int>(type: "int", nullable: false),
                    DoctorEditDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ModificationSeverity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalReportHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalReportHistories_ClinicalReports_ClinicalReportId",
                        column: x => x.ClinicalReportId,
                        principalTable: "ClinicalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportFeedbackTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatorRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportFeedbackTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportFeedbackTags_ClinicalReports_ClinicalReportId",
                        column: x => x.ClinicalReportId,
                        principalTable: "ClinicalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AiContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DoctorContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportSections_ClinicalReports_ClinicalReportId",
                        column: x => x.ClinicalReportId,
                        principalTable: "ClinicalReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalReportHistories_ClinicalReportId",
                table: "ClinicalReportHistories",
                column: "ClinicalReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalReports_ClinicalSessionId",
                table: "ClinicalReports",
                column: "ClinicalSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalSessions_BookingId",
                table: "ClinicalSessions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalSessions_DoctorId",
                table: "ClinicalSessions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalSessions_PatientId",
                table: "ClinicalSessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportFeedbackTags_ClinicalReportId",
                table: "ReportFeedbackTags",
                column: "ClinicalReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSections_ClinicalReportId_OrderIndex",
                table: "ReportSections",
                columns: new[] { "ClinicalReportId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotes_ClinicalSessionId",
                table: "SessionNotes",
                column: "ClinicalSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTranscripts_ClinicalSessionId_StartMs",
                table: "SessionTranscripts",
                columns: new[] { "ClinicalSessionId", "StartMs" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalReportHistories");

            migrationBuilder.DropTable(
                name: "ReportFeedbackTags");

            migrationBuilder.DropTable(
                name: "ReportSections");

            migrationBuilder.DropTable(
                name: "SessionNotes");

            migrationBuilder.DropTable(
                name: "SessionTranscripts");

            migrationBuilder.DropTable(
                name: "ClinicalReports");

            migrationBuilder.DropTable(
                name: "ClinicalSessions");
        }
    }
}
