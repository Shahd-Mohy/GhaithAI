using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for CrisisResourceConfig entity.
    /// Configures the primary key, properties, and relationships for crisis resource configurations.
    /// </summary>
    public class CrisisResourceConfigConfiguration : IEntityTypeConfiguration<CrisisResourceConfig>
    {
        public void Configure(EntityTypeBuilder<CrisisResourceConfig> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmergencyNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.CrisisHotline)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.CrisisTextLine)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.WebsiteUrl)
                .HasMaxLength(500);

            builder.HasOne(x => x.Country)
                .WithMany(c => c.CrisisResourceConfigs)
                .HasForeignKey(x => x.CountryCode)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
