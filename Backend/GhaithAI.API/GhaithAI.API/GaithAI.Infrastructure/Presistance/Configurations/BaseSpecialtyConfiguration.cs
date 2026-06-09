namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class BaseSpecialtyConfiguration : IEntityTypeConfiguration<BaseSpecialty>
    {
        public void Configure(EntityTypeBuilder<BaseSpecialty> builder)
        {
            builder.ToTable("BaseSpecialties");
            builder.HasKey(bs => bs.Id);
            builder.Property(bs => bs.SpecialtyName).IsRequired().HasMaxLength(150);
            builder.HasIndex(bs => bs.SpecialtyName).IsUnique();
            builder.HasMany(bs => bs.DoctorSpecialties)
                .WithOne(ds => ds.BaseSpecialty)
                .HasForeignKey(ds => ds.SpecialtyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
