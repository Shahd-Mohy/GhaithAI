using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for ChatMessage entity.
    /// Configures the primary key, properties, and relationships for chat messages.
    /// </summary>
    public class ChatMessageConfiguration
        : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(
            EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.SentimentScore)
                .HasPrecision(5, 2);

            builder.HasOne(x => x.ChatSession)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}