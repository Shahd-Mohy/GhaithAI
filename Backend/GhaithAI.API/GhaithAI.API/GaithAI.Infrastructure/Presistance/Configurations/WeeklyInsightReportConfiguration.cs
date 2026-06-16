using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for WeeklyInsightReport entity.
    /// Configures the primary key, properties, indexes, and relationships for weekly insight reports.
    /// </summary>
    public class WeeklyInsightReportConfiguration : IEntityTypeConfiguration<WeeklyInsightReport>
    {
        public void Configure(EntityTypeBuilder<WeeklyInsightReport> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AvgMoodScore)
                .HasPrecision(5, 2);

            builder.Property(x => x.BestDay)
                .HasMaxLength(50);

            builder.Property(x => x.ToughestDay)
                .HasMaxLength(50);

            builder.Property(x => x.DetectedPatterns)
                .HasMaxLength(2000);

            builder.Property(x => x.InsightSummary)
                .HasMaxLength(3000);

            builder.HasIndex(x => new { x.UserId, x.WeekStart })
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany(u => u.WeeklyInsightReports)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
