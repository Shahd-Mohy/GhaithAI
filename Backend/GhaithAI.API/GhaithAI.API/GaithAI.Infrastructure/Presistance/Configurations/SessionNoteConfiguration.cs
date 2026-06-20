namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class SessionNoteConfiguration : IEntityTypeConfiguration<SessionNote>
    {
        public void Configure(EntityTypeBuilder<SessionNote> builder)
        {
            builder.ToTable("SessionNotes");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Content).HasColumnType("nvarchar(MAX)").IsRequired();

            builder.Property(n => n.NoteType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(NoteType.Quick);
        }
    }
}
