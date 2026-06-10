global using GhaithAI.GaithAI.Domain.Entities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorsProfileConfiguration : IEntityTypeConfiguration<DoctorsProfile>
    {
        public void Configure(EntityTypeBuilder<DoctorsProfile> builder)
        {
            builder.ToTable("DoctorsProfiles");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.FullName).IsRequired().HasMaxLength(150);
            builder.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
            builder.Property(d => d.Bio).HasMaxLength(1000).IsRequired(false);
            builder.Property(d => d.YearsOfExperience).IsRequired().HasDefaultValue(0);
            builder.Property(d => d.DocumentsPdfUrl).HasMaxLength(500).IsRequired();
            builder.Property(d => d.RejectionReason).HasMaxLength(500).IsRequired(false);
            builder.Property(d => d.AverageRating).HasColumnType("real").HasDefaultValue(0.0f);
       

            builder.Property(d => d.DoctorType).HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(d => d.ApprovalStatus).HasConversion<string>().HasMaxLength(30).HasDefaultValue(ApprovalStatus.Pending);

            builder.Property(d => d.UserId).IsRequired();
            builder.HasOne(d => d.User).WithOne(u => u.DoctorsProfile) 
                .HasForeignKey<DoctorsProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
