using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for ChatSession entity.
    /// Configures the primary key, properties, and relationships for chat sessions.
    /// </summary>
    public class ChatSessionConfiguration
        : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(
            EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.HasOne(x => x.User)
                .WithMany(u => u.ChatSessions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}