namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ReportSectionConfiguration : IEntityTypeConfiguration<ReportSection>
    {
        public void Configure(EntityTypeBuilder<ReportSection> builder)
        {
            builder.ToTable("ReportSections");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title).HasMaxLength(200).IsRequired();
            builder.Property(s => s.AiContent).IsRequired();
            builder.Property(s => s.DoctorContent).IsRequired(false);
            builder.Property(s => s.SectionType).HasConversion<string>().HasMaxLength(30);

            builder.HasOne(s => s.ClinicalReport).WithMany()
                .HasForeignKey(s => s.ClinicalReportId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.ClinicalReportId, s.OrderIndex });
        }
    }
}
