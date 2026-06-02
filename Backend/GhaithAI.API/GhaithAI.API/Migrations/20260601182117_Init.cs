using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.API.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "AspNetRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Countries",
            //    columns: table => new
            //    {
            //        CountryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        CountryName = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        IsoCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Countries", x => x.CountryCode);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SelfHelpContents",
            //    columns: table => new
            //    {
            //        ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        ContentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DurationMinutes = table.Column<int>(type: "int", nullable: true),
            //        DifficultyLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SelfHelpContents", x => x.ContentId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoleClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CountryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        PreferredLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        MemoryEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        MFAEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        AcceptedAiChat = table.Column<bool>(type: "bit", nullable: false),
            //        AcceptedMoodTracking = table.Column<bool>(type: "bit", nullable: false),
            //        AcceptedDataCollection = table.Column<bool>(type: "bit", nullable: false),
            //        AcceptedTerms = table.Column<bool>(type: "bit", nullable: false),
            //        AcceptedPrivacyPolicy = table.Column<bool>(type: "bit", nullable: false),
            //        ConsentedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUsers_Countries_CountryCode",
            //            column: x => x.CountryCode,
            //            principalTable: "Countries",
            //            principalColumn: "CountryCode",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CrisisResourceConfigs",
            //    columns: table => new
            //    {
            //        ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        CountryCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        EmergencyNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CrisisHotline = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        CrisisTextLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CrisisResourceConfigs", x => x.ResourceId);
            //        table.ForeignKey(
            //            name: "FK_CrisisResourceConfigs_Countries_CountryCode",
            //            column: x => x.CountryCode,
            //            principalTable: "Countries",
            //            principalColumn: "CountryCode",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserLogins",
            //    columns: table => new
            //    {
            //        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserTokens",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ChatSessions",
            //    columns: table => new
            //    {
            //        SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        RiskLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        AISummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        EmotionalTone = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MoodChange = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        MemoryEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ChatSessions", x => x.SessionId);
            //        table.ForeignKey(
            //            name: "FK_ChatSessions_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "EmergencyContacts",
            //    columns: table => new
            //    {
            //        ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        PriorityOrder = table.Column<int>(type: "int", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_EmergencyContacts", x => x.ContactId);
            //        table.ForeignKey(
            //            name: "FK_EmergencyContacts_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "JournalEntries",
            //    columns: table => new
            //    {
            //        JournalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        PromptType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        WordCount = table.Column<int>(type: "int", nullable: false),
            //        MoodBefore = table.Column<int>(type: "int", nullable: false),
            //        MoodAfter = table.Column<int>(type: "int", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_JournalEntries", x => x.JournalId);
            //        table.ForeignKey(
            //            name: "FK_JournalEntries_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MoodLogs",
            //    columns: table => new
            //    {
            //        MoodLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        MoodScore = table.Column<int>(type: "int", nullable: false),
            //        EmotionTags = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        StressLevel = table.Column<int>(type: "int", nullable: false),
            //        SleepQuality = table.Column<int>(type: "int", nullable: false),
            //        Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MoodLogs", x => x.MoodLogId);
            //        table.ForeignKey(
            //            name: "FK_MoodLogs_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserActivities",
            //    columns: table => new
            //    {
            //        ActivityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SelfHelpContentContentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ActivityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DurationSeconds = table.Column<int>(type: "int", nullable: false),
            //        ProgressPercent = table.Column<int>(type: "int", nullable: false),
            //        Completed = table.Column<bool>(type: "bit", nullable: false),
            //        CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserActivities", x => x.ActivityId);
            //        table.ForeignKey(
            //            name: "FK_UserActivities_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_UserActivities_SelfHelpContents_SelfHelpContentContentId",
            //            column: x => x.SelfHelpContentContentId,
            //            principalTable: "SelfHelpContents",
            //            principalColumn: "ContentId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "WeeklyInsightReports",
            //    columns: table => new
            //    {
            //        ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        WeekEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        AvgMoodScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        BestDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ToughestDay = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DetectedPatterns = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        InsightSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_WeeklyInsightReports", x => x.ReportId);
            //        table.ForeignKey(
            //            name: "FK_WeeklyInsightReports_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ChatMessages",
            //    columns: table => new
            //    {
            //        MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        ChatSessionSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        SentimentScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        DetectedEmotion = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DetectedLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
            //        table.ForeignKey(
            //            name: "FK_ChatMessages_ChatSessions_ChatSessionSessionId",
            //            column: x => x.ChatSessionSessionId,
            //            principalTable: "ChatSessions",
            //            principalColumn: "SessionId",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "RiskEvents",
            //    columns: table => new
            //    {
            //        RiskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        RiskType = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        DetectedMarkers = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SupportingContext = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            //        AIActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsDeletable = table.Column<bool>(type: "bit", nullable: false),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_RiskEvents", x => x.RiskId);
            //        table.ForeignKey(
            //            name: "FK_RiskEvents_ChatMessages_MessageId",
            //            column: x => x.MessageId,
            //            principalTable: "ChatMessages",
            //            principalColumn: "MessageId");
            //        table.ForeignKey(
            //            name: "FK_RiskEvents_ChatSessions_SessionId",
            //            column: x => x.SessionId,
            //            principalTable: "ChatSessions",
            //            principalColumn: "SessionId");
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoleClaims_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "RoleNameIndex",
            //    table: "AspNetRoles",
            //    column: "NormalizedName",
            //    unique: true,
            //    filter: "[NormalizedName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserClaims_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserLogins_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "EmailIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedEmail");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUsers_CountryCode",
            //    table: "AspNetUsers",
            //    column: "CountryCode");

            //migrationBuilder.CreateIndex(
            //    name: "UserNameIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedUserName",
            //    unique: true,
            //    filter: "[NormalizedUserName] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ChatMessages_ChatSessionSessionId",
            //    table: "ChatMessages",
            //    column: "ChatSessionSessionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ChatSessions_UserId",
            //    table: "ChatSessions",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Countries_CountryName",
            //    table: "Countries",
            //    column: "CountryName",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_CrisisResourceConfigs_CountryCode",
            //    table: "CrisisResourceConfigs",
            //    column: "CountryCode");

            //migrationBuilder.CreateIndex(
            //    name: "IX_EmergencyContacts_UserId",
            //    table: "EmergencyContacts",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_JournalEntries_UserId",
            //    table: "JournalEntries",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_MoodLogs_UserId",
            //    table: "MoodLogs",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_RiskEvents_MessageId",
            //    table: "RiskEvents",
            //    column: "MessageId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_RiskEvents_SessionId",
            //    table: "RiskEvents",
            //    column: "SessionId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserActivities_SelfHelpContentContentId",
            //    table: "UserActivities",
            //    column: "SelfHelpContentContentId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_UserActivities_UserId",
            //    table: "UserActivities",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_WeeklyInsightReports_UserId_WeekStart",
            //    table: "WeeklyInsightReports",
            //    columns: new[] { "UserId", "WeekStart" },
            //    unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CrisisResourceConfigs");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "MoodLogs");

            migrationBuilder.DropTable(
                name: "RiskEvents");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "WeeklyInsightReports");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "SelfHelpContents");

            migrationBuilder.DropTable(
                name: "ChatSessions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
