namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ReportFeedbackTagConfiguration : IEntityTypeConfiguration<ReportFeedbackTag>
    {
        public void Configure(EntityTypeBuilder<ReportFeedbackTag> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.TagType)
                .HasConversion<string>()
                .HasMaxLength(40);

            builder.Property(f => f.CreatorRole)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(f => f.CreatorId)
                .HasMaxLength(450) // نفس مقاس Identity user id
                .IsRequired();

            builder.Property(f => f.Notes)
                .HasColumnType("nvarchar(MAX)");

            builder.HasIndex(f => f.ClinicalReportId);
        }
    }
}

