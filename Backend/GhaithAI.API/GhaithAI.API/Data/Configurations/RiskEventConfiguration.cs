using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    public class RiskEventConfiguration
        : IEntityTypeConfiguration<RiskEvent>
    {
        public void Configure(
            EntityTypeBuilder<RiskEvent> builder)
        {
            builder.HasKey(x => x.RiskId);

            builder.Property(x => x.ConfidenceScore)
                .HasPrecision(5, 2);

            builder.HasOne(x => x.ChatSession)
                .WithMany(x => x.RiskEvents)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ChatMessage)
                .WithMany(x => x.RiskEvents)
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}