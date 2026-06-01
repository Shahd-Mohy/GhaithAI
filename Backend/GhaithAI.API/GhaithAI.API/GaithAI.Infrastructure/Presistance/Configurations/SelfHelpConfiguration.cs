using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for SelfHelpContent entity.
    /// Configures the primary key, properties, and relationships for self-help content.
    /// </summary>
    public class SelfHelpConfiguration
        : IEntityTypeConfiguration<SelfHelpContent>
    {
        public void Configure(
            EntityTypeBuilder<SelfHelpContent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(3000);
        }
    }
}