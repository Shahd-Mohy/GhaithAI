namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicalReportConfiguration : IEntityTypeConfiguration<ClinicalReport>
    {
        public void Configure(EntityTypeBuilder<ClinicalReport> builder)
        {
            builder.ToTable("ClinicalReports");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReportType).HasMaxLength(100).IsRequired();
            builder.Property(r => r.AiDraftJson).IsRequired();
            builder.Property(r => r.FinalContent).IsRequired();
            builder.Property(r => r.DoctorNotes).IsRequired(false);
            builder.Property(r => r.AiModelVersion).HasMaxLength(100).IsRequired();
            builder.Property(r => r.PdfUrl).HasMaxLength(500).IsRequired(false);

            builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(ReportStatus.Draft);

            builder.HasOne(r => r.ClinicalSession).WithMany()
                .HasForeignKey(r => r.ClinicalSessionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => r.ClinicalSessionId).IsUnique();
        }
    }
}
