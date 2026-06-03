using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GhaithAI.API.Migrations
{
    /// <inheritdoc />
    public partial class FixJournalEntryMoodTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // JournalEntries already has the correct schema in the database:
            // - MoodBefore: nvarchar(max)
            // - MoodAfter:  nvarchar(max)
            // - Tags:       nvarchar(max) NULL
            // No changes needed.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: nothing was changed in Up().
        }
    }
}