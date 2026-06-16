namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorLanguageConfiguration : IEntityTypeConfiguration<DoctorLanguage>
    {
        public void Configure(EntityTypeBuilder<DoctorLanguage> builder)
        {
            builder.ToTable("DoctorLanguages");
            builder.HasKey(dl => dl.Id);

            builder.HasOne(dl => dl.Doctor).WithMany(d => d.DoctorLanguages) 
                .HasForeignKey(dl => dl.DoctorId).OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dl => dl.BaseLanguage).WithMany(bl => bl.DoctorLanguages)
                .HasForeignKey(dl => dl.BaseLanguageId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(dl => new { dl.DoctorId, dl.BaseLanguageId }).IsUnique();
        }
    } 
}

