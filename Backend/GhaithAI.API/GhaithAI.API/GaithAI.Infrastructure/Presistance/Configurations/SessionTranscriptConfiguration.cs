namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class SessionTranscriptConfiguration : IEntityTypeConfiguration<SessionTranscript>
    {
        public void Configure(EntityTypeBuilder<SessionTranscript> builder)
        {
            builder.ToTable("SessionTranscripts");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Content).IsRequired();
            builder.Property(t => t.ConfidenceScore).HasColumnType("decimal(5,2)").IsRequired(false);
            builder.Property(t => t.Speaker).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(t => t.ClinicalSession).WithMany()
                .HasForeignKey(t => t.ClinicalSessionId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.ClinicalSessionId, t.StartMs });
        }
    }
}
