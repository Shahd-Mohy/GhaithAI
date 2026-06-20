namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicalSessionConfiguration : IEntityTypeConfiguration<ClinicalSession>
    {
        public void Configure(EntityTypeBuilder<ClinicalSession> builder)
        {
            builder.ToTable("ClinicalSessions");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Provider).HasMaxLength(100).IsRequired();
            builder.Property(s => s.ChiefComplaint).HasMaxLength(500).IsRequired(false);
            builder.Property(s => s.VideoRoomId).HasMaxLength(200).IsRequired(false);
            builder.Property(s => s.VideoRoomUrl).HasMaxLength(500).IsRequired(false);

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ClinicalSessionStatus.InProgress);

            builder.Property(s => s.SessionType)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.HasOne(s => s.Booking)
                .WithMany()
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Doctor)
                .WithMany()
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.Patient)
                .WithMany()
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Notes)
                .WithOne(n => n.ClinicalSession)
                .HasForeignKey(n => n.ClinicalSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
