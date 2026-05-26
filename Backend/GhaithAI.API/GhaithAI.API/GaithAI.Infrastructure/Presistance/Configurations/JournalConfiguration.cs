using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class JournalConfiguration
        : IEntityTypeConfiguration<JournalEntry>
    {
        public void Configure(
            EntityTypeBuilder<JournalEntry> builder)
        {
            builder.HasKey(x => x.JournalId);

            builder.Property(x => x.Content)
                .IsRequired();
        }
    }
}