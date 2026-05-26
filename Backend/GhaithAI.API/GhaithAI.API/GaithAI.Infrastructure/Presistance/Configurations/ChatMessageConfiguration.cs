using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class ChatMessageConfiguration
        : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(
            EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(x => x.MessageId);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.SentimentScore)
                .HasPrecision(5, 2);
        }
    }
}