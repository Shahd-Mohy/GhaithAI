namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BookingDate).HasColumnType("date").IsRequired();
            builder.Property(b => b.SlotTime).IsRequired();
            builder.Property(b => b.Notes).HasMaxLength(1000).IsRequired(false);
            builder.Property(b => b.ConfirmedBy).HasMaxLength(150).IsRequired(false);
            builder.Property(b => b.CancelledBy).HasMaxLength(150).IsRequired(false);

            builder.Property(b => b.SessionType).HasConversion<string>().HasMaxLength(30).HasDefaultValue(SessionType.Offline);
            builder.Property(b => b.BookingSource).HasConversion<string>().HasMaxLength(30).HasDefaultValue(BookingSource.App);
            builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(BookingStatus.Confirmed);

            builder.Property(b => b.PatientId).IsRequired(false);
            builder.Property(b => b.ClinicPatientId).IsRequired(false);


            builder.HasOne(b => b.Clinic).WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClinicId).OnDelete(DeleteBehavior.Cascade);
 
            builder.HasOne(b => b.Patient).WithMany()
                .HasForeignKey(b => b.PatientId).OnDelete(DeleteBehavior.Restrict);
           
            builder.HasOne(b => b.ClinicPatient).WithMany(cp => cp.Bookings)
                .HasForeignKey(b => b.ClinicPatientId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
                
