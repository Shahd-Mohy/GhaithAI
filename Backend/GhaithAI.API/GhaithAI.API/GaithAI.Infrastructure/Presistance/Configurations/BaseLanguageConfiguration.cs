namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class BaseLanguageConfiguration : IEntityTypeConfiguration<BaseLanguage>
    {
        public void Configure(EntityTypeBuilder<BaseLanguage> builder)
        {
            builder.ToTable("BaseLanguages");
            builder.HasKey(bl => bl.Id);
            builder.Property(bl => bl.LanguageName).IsRequired().HasMaxLength(100);
            builder.HasIndex(bl => bl.LanguageName) .IsUnique();
            builder.HasMany(bl => bl.DoctorLanguages)
                .WithOne(dl => dl.BaseLanguage)
                .HasForeignKey(dl => dl.BaseLanguageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
