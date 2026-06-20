namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class ReportFeedbackTagConfiguration : IEntityTypeConfiguration<ReportFeedbackTag>
    {
        public void Configure(EntityTypeBuilder<ReportFeedbackTag> builder)
        {
            builder.ToTable("ReportFeedbackTags");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.CreatorRole).HasMaxLength(50).IsRequired();
            builder.Property(t => t.Notes).HasMaxLength(1000).IsRequired(false);
            builder.Property(t => t.TagType).HasConversion<string>().HasMaxLength(30);

            builder.HasOne(t => t.ClinicalReport).WithMany()
                .HasForeignKey(t => t.ClinicalReportId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
