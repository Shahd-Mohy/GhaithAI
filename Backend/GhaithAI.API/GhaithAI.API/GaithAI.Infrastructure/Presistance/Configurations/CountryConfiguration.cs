using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for Country entity.
    /// Configures the primary key, indexes, and relationships for the global country reference table.
    /// </summary>
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CountryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.IsoCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.HasIndex(x => x.CountryName)
                .IsUnique();

            builder.HasMany(x => x.Users)
                .WithOne(u => u.Country)
                .HasForeignKey(u => u.CountryCode)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.CrisisResourceConfigs)
                .WithOne(c => c.Country)
                .HasForeignKey(c => c.CountryCode)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
