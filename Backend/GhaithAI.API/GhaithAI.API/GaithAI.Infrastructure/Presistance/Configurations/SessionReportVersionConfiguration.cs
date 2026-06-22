using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class SessionReportVersionConfiguration : IEntityTypeConfiguration<SessionReportVersion>
    {
        public void Configure(EntityTypeBuilder<SessionReportVersion> builder)
        {
            builder.ToTable("SessionReportVersions");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.SnapshotJson)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            builder.Property(v => v.ChangeNote).HasMaxLength(1000).IsRequired(false);
            builder.Property(v => v.CreatedBy).HasMaxLength(100).IsRequired();

            builder.HasOne(v => v.SessionReport)
                   .WithMany(r => r.Versions)
                   .HasForeignKey(v => v.SessionReportId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
