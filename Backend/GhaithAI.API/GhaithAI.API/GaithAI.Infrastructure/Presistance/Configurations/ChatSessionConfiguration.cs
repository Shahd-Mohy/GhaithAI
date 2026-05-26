using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class ChatSessionConfiguration
        : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(
            EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(x => x.SessionId);

            builder.Property(x => x.Title)
                .HasMaxLength(200);
        }
    }
}