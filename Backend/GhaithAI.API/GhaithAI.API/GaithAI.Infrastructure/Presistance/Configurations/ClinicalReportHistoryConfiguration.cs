namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicalReportHistoryConfiguration : IEntityTypeConfiguration<ClinicalReportHistory>
    {
        public void Configure(EntityTypeBuilder<ClinicalReportHistory> builder)
        {
            builder.HasKey(h => h.Id);

            // FK,UQ — كل ClinicalReport عنده History واحدة بالظبط
            builder.HasIndex(h => h.ClinicalReportId)
                .IsUnique();

            builder.Property(h => h.OriginalAiContent)
                .HasColumnType("nvarchar(MAX)")
                .IsRequired();

            builder.Property(h => h.ModificationSeverity)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(h => h.TotalChangesCount)
                .HasDefaultValue(0);

            builder.Property(h => h.CapturedAt)
                .IsRequired();
        }
    }
}
