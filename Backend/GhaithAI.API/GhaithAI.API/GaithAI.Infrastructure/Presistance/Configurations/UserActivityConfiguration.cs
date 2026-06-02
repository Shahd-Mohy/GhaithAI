using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for UserActivity entity.
    /// Configures the primary key, properties, and relationships for user activities.
    /// </summary>
    public class UserActivityConfiguration : IEntityTypeConfiguration<UserActivity>
    {
        public void Configure(EntityTypeBuilder<UserActivity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActivityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Metadata)
                .HasMaxLength(4000);

            builder.HasOne(x => x.User)
                .WithMany(u => u.UserActivities)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.SelfHelpContent)
                .WithMany(c => c.UserActivities)
                .HasForeignKey(x => x.ContentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
