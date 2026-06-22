using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionReportMVP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalReportHistories");

            migrationBuilder.DropTable(
                name: "ReportFeedbackTags");

            migrationBuilder.DropTable(
                name: "ReportSections");

            migrationBuilder.DropTable(
                name: "ClinicalReports");

            migrationBuilder.CreateTable(
                name: "SessionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClinicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RiskTier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SiPresent = table.Column<bool>(type: "bit", nullable: false),
                    SoapSubjective = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoapObjective = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoapAssessment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoapPlan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReports_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_ClinicalSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ClinicalSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionReports_DoctorsProfiles_ClinicianId",
                        column: x => x.ClinicianId,
                        principalTable: "DoctorsProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionReportVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionReportVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionReportVersions_SessionReports_SessionReportId",
                        column: x => x.SessionReportId,
                        principalTable: "SessionReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_ClinicianId",
                table: "SessionReports",
                column: "ClinicianId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_PatientId",
                table: "SessionReports",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_RiskTier",
                table: "SessionReports",
                column: "RiskTier");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_SessionId",
                table: "SessionReports",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReports_SiPresent",
                table: "SessionReports",
                column: "SiPresent");

            migrationBuilder.CreateIndex(
                name: "IX_SessionReportVersions_SessionReportId",
                table: "SessionReportVersions",
                column: "SessionReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionReportVersions");

            migrationBuilder.DropTable(
                name: "SessionReports");

            migrationBuilder.CreateTable(
                name: "ClinicalReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AiDraftJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "ClinicalReportHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorEditDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModificationSeverity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OriginalAiContent = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    TotalChangesCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatorRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    TagType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    AiContent = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoctorContent = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    SectionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_ReportFeedbackTags_ClinicalReportId",
                table: "ReportFeedbackTags",
                column: "ClinicalReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportSections_ClinicalReportId_OrderIndex",
                table: "ReportSections",
                columns: new[] { "ClinicalReportId", "OrderIndex" });
        }
    }
}
