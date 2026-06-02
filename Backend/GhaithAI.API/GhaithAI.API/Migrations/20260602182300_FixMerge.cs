using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.API.Migrations
{
    /// <inheritdoc />
    public partial class FixMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_UserActivities_SelfHelpContentContentId",
            //    table: "UserActivities");

            //migrationBuilder.DropIndex(
            //    name: "IX_ChatMessages_ChatSessionSessionId",
            //    table: "ChatMessages");

            //migrationBuilder.DropColumn(
            //    name: "GeneratedAt",
            //    table: "WeeklyInsightReports");

            //migrationBuilder.DropColumn(
            //    name: "ReportId",
            //    table: "WeeklyInsightReports");

            //migrationBuilder.DropColumn(
            //    name: "ActivityId",
            //    table: "UserActivities");

            //migrationBuilder.DropColumn(
            //    name: "SelfHelpContentContentId",
            //    table: "UserActivities");

            //migrationBuilder.DropColumn(
            //    name: "ContentId",
            //    table: "SelfHelpContents");

            //migrationBuilder.DropColumn(
            //    name: "RiskId",
            //    table: "RiskEvents");

            //migrationBuilder.DropColumn(
            //    name: "JournalId",
            //    table: "JournalEntries");

            //migrationBuilder.DropColumn(
            //    name: "ContactId",
            //    table: "EmergencyContacts");

            //migrationBuilder.DropColumn(
            //    name: "ResourceId",
            //    table: "CrisisResourceConfigs");

            //migrationBuilder.DropColumn(
            //    name: "CountryCode",
            //    table: "Countries");

            //migrationBuilder.DropColumn(
            //    name: "SessionId",
            //    table: "ChatSessions");

            //migrationBuilder.DropColumn(
            //    name: "StartedAt",
            //    table: "ChatSessions");

            //migrationBuilder.DropColumn(
            //    name: "ChatSessionSessionId",
            //    table: "ChatMessages");

            //migrationBuilder.DropColumn(
            //    name: "MessageId",
            //    table: "ChatMessages");

            //migrationBuilder.DropColumn(
            //    name: "SentAt",
            //    table: "ChatMessages");

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "AvgMoodScore",
            //    table: "WeeklyInsightReports",
            //    type: "decimal(18,4)",
            //    precision: 5,
            //    scale: 2,
            //    nullable: true,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(5,2)",
            //    oldPrecision: 5,
            //    oldScale: 2,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "ConfidenceScore",
            //    table: "RiskEvents",
            //    type: "decimal(18,4)",
            //    precision: 5,
            //    scale: 2,
            //    nullable: true,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(5,2)",
            //    oldPrecision: 5,
            //    oldScale: 2,
            //    oldNullable: true);

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsDeleted",
            //    table: "MoodLogs",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AlterColumn<decimal>(
            //    name: "SentimentScore",
            //    table: "ChatMessages",
            //    type: "decimal(18,4)",
            //    precision: 5,
            //    scale: 2,
            //    nullable: true,
            //    oldClrType: typeof(decimal),
            //    oldType: "decimal(5,2)",
            //    oldPrecision: 5,
            //    oldScale: 2,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "CountryCode",
            //    table: "AspNetUsers",
            //    type: "nvarchar(450)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MoodLogs");

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgMoodScore",
                table: "WeeklyInsightReports",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "WeeklyInsightReports",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ReportId",
                table: "WeeklyInsightReports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityId",
                table: "UserActivities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SelfHelpContentContentId",
                table: "UserActivities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ContentId",
                table: "SelfHelpContents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfidenceScore",
                table: "RiskEvents",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RiskId",
                table: "RiskEvents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "JournalId",
                table: "JournalEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "EmergencyContacts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceId",
                table: "CrisisResourceConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Countries",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "ChatSessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "SentimentScore",
                table: "ChatMessages",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChatSessionSessionId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "ChatMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_SelfHelpContentContentId",
                table: "UserActivities",
                column: "SelfHelpContentContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionSessionId",
                table: "ChatMessages",
                column: "ChatSessionSessionId");
        }
    }
}
