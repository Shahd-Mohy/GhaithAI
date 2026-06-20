namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ReportSectionConfiguration : IEntityTypeConfiguration<ReportSection>
    {
        public void Configure(EntityTypeBuilder<ReportSection> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.SectionType)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(s => s.Title)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(s => s.AiContent)
                .HasColumnType("nvarchar(MAX)")
                .IsRequired();

            builder.Property(s => s.DoctorContent)
                .HasColumnType("nvarchar(MAX)");

            builder.HasIndex(s => new { s.ClinicalReportId, s.OrderIndex });
        }
    }
}

