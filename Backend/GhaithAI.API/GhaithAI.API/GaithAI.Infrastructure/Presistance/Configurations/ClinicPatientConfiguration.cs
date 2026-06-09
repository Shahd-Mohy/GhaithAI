namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicPatientConfiguration : IEntityTypeConfiguration<ClinicPatient>
    {
        public void Configure(EntityTypeBuilder<ClinicPatient> builder)
        {
            builder.ToTable("ClinicPatients");
            builder.HasKey(cp => cp.Id);
            builder.Property(cp => cp.PatientFullName).IsRequired().HasMaxLength(150);
            builder.Property(cp => cp.PatientPhone).IsRequired().HasMaxLength(11);
            builder.Property(cp => cp.Notes).HasMaxLength(1000).IsRequired(false);

            builder.HasOne(cp => cp.Clinic).WithMany(c => c.ClinicPatients)
                .HasForeignKey(cp => cp.ClinicId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
