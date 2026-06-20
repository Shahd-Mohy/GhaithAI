namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicalReportHistoryConfiguration : IEntityTypeConfiguration<ClinicalReportHistory>
    {
        public void Configure(EntityTypeBuilder<ClinicalReportHistory> builder)
        {
            builder.ToTable("ClinicalReportHistories");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.OriginalAiContent).IsRequired();
            builder.Property(h => h.ModificationSeverity).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(h => h.ClinicalReport).WithMany()
                .HasForeignKey(h => h.ClinicalReportId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(h => h.ClinicalReportId).IsUnique();
        }
    }
}
