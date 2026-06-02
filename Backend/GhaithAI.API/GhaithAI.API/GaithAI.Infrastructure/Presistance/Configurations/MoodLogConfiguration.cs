using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for MoodLog entity.
    /// Configures the primary key, properties, and relationships for mood log entries.
    /// </summary>
    public class MoodLogConfiguration
        : IEntityTypeConfiguration<MoodLog>
    {
        public void Configure(
            EntityTypeBuilder<MoodLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            builder.HasOne(x => x.User)
                .WithMany(u => u.MoodLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}