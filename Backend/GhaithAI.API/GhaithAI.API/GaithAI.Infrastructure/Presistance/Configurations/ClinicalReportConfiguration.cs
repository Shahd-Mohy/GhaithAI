namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ClinicalReportConfiguration : IEntityTypeConfiguration<ClinicalReport>
    {
        public void Configure(EntityTypeBuilder<ClinicalReport> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.ReportType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.AiDraftJson)
                .HasColumnType("nvarchar(MAX)");

            builder.Property(r => r.FinalContent)
                .HasColumnType("nvarchar(MAX)");

            builder.Property(r => r.DoctorNotes)
                .HasColumnType("nvarchar(MAX)");

            builder.Property(r => r.AiModelVersion)
                .HasMaxLength(100);

            builder.HasOne(r => r.ClinicalSession)
                .WithOne(s => s.ClinicalReport)
                .HasForeignKey<ClinicalReport>(r => r.ClinicalSessionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(r => r.ReportSections)
                .WithOne(s => s.ClinicalReport)
                .HasForeignKey(s => s.ClinicalReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── علاقة 1:1 إجبارية مع History ──
            // ملاحظة: في الـ ERD كانت موضوعة كـ conditional 0/1، لكن قرارنا
            // إنها إجبارية — أي تقرير AI لازم يكون عنده History record
            // من لحظة التوليد (ReportGenerationService بينشئهم في نفس الوقت).
            builder.HasOne(r => r.ClinicalReportHistory)
                .WithOne(h => h.ClinicalReport)
                .HasForeignKey<ClinicalReportHistory>(h => h.ClinicalReportId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.FeedbackTags)
                .WithOne(f => f.ClinicalReport)
                .HasForeignKey(f => f.ClinicalReportId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
