using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class SelfHelpConfiguration
        : IEntityTypeConfiguration<SelfHelpContent>
    {
        public void Configure(
            EntityTypeBuilder<SelfHelpContent> builder)
        {
            builder.HasKey(x => x.ContentId);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(3000);
        }
    }
}