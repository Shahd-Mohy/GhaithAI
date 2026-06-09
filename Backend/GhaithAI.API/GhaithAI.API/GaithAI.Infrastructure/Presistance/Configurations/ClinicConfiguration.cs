namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.ToTable("Clinics");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClinicName).IsRequired().HasMaxLength(150);
            builder.Property(c => c.ClinicAddress).IsRequired().HasMaxLength(250);
            builder.Property(c => c.City).IsRequired().HasMaxLength(50);
            builder.Property(c => c.CountryCode).HasMaxLength(10).IsRequired(false); 
            builder.Property(c => c.Phone).IsRequired().HasMaxLength(11);
            builder.Property(c => c.ContactEmail).HasMaxLength(150).IsRequired(false);
            builder.Property(c => c.FeePerSession).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(c => c.SessionDurationMinutes).IsRequired().HasDefaultValue(45); 
            builder.Property(c => c.IsPublicListed).IsRequired().HasDefaultValue(true);


            builder.HasOne(c => c.Doctor).WithOne(d => d.Clinic) 
                .HasForeignKey<Clinic>(c => c.DoctorId).OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
