namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorReviewConfiguration : IEntityTypeConfiguration<DoctorReview>
    {
        public void Configure(EntityTypeBuilder<DoctorReview> builder)
        {
            builder.ToTable("DoctorReviews");
            builder.HasKey(dr => dr.Id);
            builder.Property(dr => dr.Rating).IsRequired();
            builder.Property(dr => dr.Comment).HasMaxLength(500).IsRequired(false);

            builder.HasOne(dr => dr.Doctor).WithMany(c => c.Reviews)
                .HasForeignKey(dr => dr.DoctorId).OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dr => dr.Patient).WithMany(dr=> dr.DoctorReviews)
                .HasForeignKey(dr => dr.PatientId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(dr => new { dr.PatientId, dr.DoctorId }).IsUnique();

        }
    }
}
