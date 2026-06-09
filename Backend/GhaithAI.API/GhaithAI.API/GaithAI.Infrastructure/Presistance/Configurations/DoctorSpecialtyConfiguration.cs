namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorSpecialtyConfiguration : IEntityTypeConfiguration<DoctorSpecialty>
    {
        public void Configure(EntityTypeBuilder<DoctorSpecialty> builder)
        {
            builder.ToTable("DoctorSpecialties");
            builder.HasKey(ds => ds.Id);

            builder.HasOne(ds => ds.Doctor).WithMany(d => d.DoctorSpecialties)
                .HasForeignKey(ds => ds.DoctorId).OnDelete(DeleteBehavior.Cascade); 

            builder.HasOne(ds => ds.BaseSpecialty).WithMany(bs => bs.DoctorSpecialties) 
                .HasForeignKey(ds => ds.SpecialtyId).OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
