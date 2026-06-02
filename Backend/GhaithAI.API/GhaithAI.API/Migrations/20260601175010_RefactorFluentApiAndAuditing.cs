using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFluentApiAndAuditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionSessionId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_AspNetUsers_UserId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CrisisResourceConfigs_Countries_CountryCode",
                table: "CrisisResourceConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_AspNetUsers_UserId",
                table: "EmergencyContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodLogs_AspNetUsers_UserId",
                table: "MoodLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskEvents_ChatMessages_MessageId",
                table: "RiskEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_AspNetUsers_UserId",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_SelfHelpContents_SelfHelpContentContentId",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyInsightReports_AspNetUsers_UserId",
                table: "WeeklyInsightReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserActivities_SelfHelpContentContentId",
                table: "UserActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ChatSessionSessionId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "GeneratedAt",
                table: "WeeklyInsightReports",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "WeeklyInsightReports",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SelfHelpContentContentId",
                table: "UserActivities",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "SelfHelpContents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "RiskId",
                table: "RiskEvents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LoggedAt",
                table: "MoodLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "MoodLogId",
                table: "MoodLogs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "JournalId",
                table: "JournalEntries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "EmergencyContacts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "CrisisResourceConfigs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "Countries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "ChatSessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "ChatSessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "ChatMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ChatSessionSessionId",
                table: "ChatMessages",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "ToughestDay",
                table: "WeeklyInsightReports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InsightSummary",
                table: "WeeklyInsightReports",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetectedPatterns",
                table: "WeeklyInsightReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BestDay",
                table: "WeeklyInsightReports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgMoodScore",
                table: "WeeklyInsightReports",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "UserActivities",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "UserActivities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SelfHelpContents",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SelfHelpContents",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SelfHelpContents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SelfHelpContents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SelfHelpContents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SelfHelpContents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfidenceScore",
                table: "RiskEvents",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RiskEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RiskEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RiskEvents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "RiskEvents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MoodLogs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Relationship",
                table: "EmergencyContacts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmergencyContacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "EmergencyContacts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EmergencyContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmergencyContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EmergencyContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "EmergencyContacts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "WebsiteUrl",
                table: "CrisisResourceConfigs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyNumber",
                table: "CrisisResourceConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CrisisTextLine",
                table: "CrisisResourceConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CrisisHotline",
                table: "CrisisResourceConfigs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "IsoCode",
                table: "Countries",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CountryName",
                table: "Countries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Countries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "Title",
            //    table: "ChatSessions",
            //    type: "nvarchar(200)",
            //    maxLength: 200,
            //    nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ChatSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ChatSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SentimentScore",
                table: "ChatMessages",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_ContentId",
                table: "UserActivities",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_AspNetUsers_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrisisResourceConfigs_Countries_CountryCode",
                table: "CrisisResourceConfigs",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_AspNetUsers_UserId",
                table: "EmergencyContacts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MoodLogs_AspNetUsers_UserId",
                table: "MoodLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RiskEvents_ChatMessages_MessageId",
                table: "RiskEvents",
                column: "MessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_AspNetUsers_UserId",
                table: "UserActivities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_SelfHelpContents_ContentId",
                table: "UserActivities",
                column: "ContentId",
                principalTable: "SelfHelpContents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyInsightReports_AspNetUsers_UserId",
                table: "WeeklyInsightReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatSessions_AspNetUsers_UserId",
                table: "ChatSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CrisisResourceConfigs_Countries_CountryCode",
                table: "CrisisResourceConfigs");

            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyContacts_AspNetUsers_UserId",
                table: "EmergencyContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_MoodLogs_AspNetUsers_UserId",
                table: "MoodLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_RiskEvents_ChatMessages_MessageId",
                table: "RiskEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_AspNetUsers_UserId",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActivities_SelfHelpContents_ContentId",
                table: "UserActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyInsightReports_AspNetUsers_UserId",
                table: "WeeklyInsightReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserActivities_ContentId",
                table: "UserActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SelfHelpContents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SelfHelpContents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SelfHelpContents");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SelfHelpContents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RiskEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RiskEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RiskEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RiskEvents");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ChatSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ChatSessions");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "WeeklyInsightReports",
                newName: "GeneratedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WeeklyInsightReports",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserActivities",
                newName: "SelfHelpContentContentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SelfHelpContents",
                newName: "ContentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RiskEvents",
                newName: "RiskId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "MoodLogs",
                newName: "LoggedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MoodLogs",
                newName: "MoodLogId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "JournalEntries",
                newName: "JournalId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EmergencyContacts",
                newName: "ContactId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CrisisResourceConfigs",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Countries",
                newName: "CountryCode");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatSessions",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ChatSessions",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatMessages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ChatMessages",
                newName: "ChatSessionSessionId");

            migrationBuilder.AlterColumn<string>(
                name: "ToughestDay",
                table: "WeeklyInsightReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InsightSummary",
                table: "WeeklyInsightReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetectedPatterns",
                table: "WeeklyInsightReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BestDay",
                table: "WeeklyInsightReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AvgMoodScore",
                table: "WeeklyInsightReports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Metadata",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityType",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityId",
                table: "UserActivities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SelfHelpContents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SelfHelpContents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConfidenceScore",
                table: "RiskEvents",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MoodLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "JournalEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Relationship",
                table: "EmergencyContacts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "EmergencyContacts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "EmergencyContacts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "WebsiteUrl",
                table: "CrisisResourceConfigs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyNumber",
                table: "CrisisResourceConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CrisisTextLine",
                table: "CrisisResourceConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CrisisHotline",
                table: "CrisisResourceConfigs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "IsoCode",
                table: "Countries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "CountryName",
                table: "Countries",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "SentimentScore",
                table: "ChatMessages",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MessageId",
                table: "ChatMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActivities",
                table: "UserActivities",
                column: "ActivityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_SelfHelpContentContentId",
                table: "UserActivities",
                column: "SelfHelpContentContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionSessionId",
                table: "ChatMessages",
                column: "ChatSessionSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Countries_CountryCode",
                table: "AspNetUsers",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "CountryCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_ChatSessionSessionId",
                table: "ChatMessages",
                column: "ChatSessionSessionId",
                principalTable: "ChatSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatSessions_AspNetUsers_UserId",
                table: "ChatSessions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CrisisResourceConfigs_Countries_CountryCode",
                table: "CrisisResourceConfigs",
                column: "CountryCode",
                principalTable: "Countries",
                principalColumn: "CountryCode",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyContacts_AspNetUsers_UserId",
                table: "EmergencyContacts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_AspNetUsers_UserId",
                table: "JournalEntries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MoodLogs_AspNetUsers_UserId",
                table: "MoodLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RiskEvents_ChatMessages_MessageId",
                table: "RiskEvents",
                column: "MessageId",
                principalTable: "ChatMessages",
                principalColumn: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_AspNetUsers_UserId",
                table: "UserActivities",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActivities_SelfHelpContents_SelfHelpContentContentId",
                table: "UserActivities",
                column: "SelfHelpContentContentId",
                principalTable: "SelfHelpContents",
                principalColumn: "ContentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyInsightReports_AspNetUsers_UserId",
                table: "WeeklyInsightReports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
