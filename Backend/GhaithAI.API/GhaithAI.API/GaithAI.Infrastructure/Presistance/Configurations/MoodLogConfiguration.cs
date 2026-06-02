using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class MoodLogConfiguration
        : IEntityTypeConfiguration<MoodLog>
    {
        public void Configure(
            EntityTypeBuilder<MoodLog> builder)
        {
            builder.HasKey(x => x.MoodLogId);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);
        }
    }
}