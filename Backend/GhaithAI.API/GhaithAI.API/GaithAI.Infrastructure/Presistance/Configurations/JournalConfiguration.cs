using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for JournalEntry entity.
    /// Configures the primary key, properties, and relationships for journal entries.
    /// </summary>
    public class JournalConfiguration
        : IEntityTypeConfiguration<JournalEntry>
    {
        public void Configure(
            EntityTypeBuilder<JournalEntry> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(u => u.JournalEntries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}