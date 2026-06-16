namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.UserId).IsRequired().HasMaxLength(450);
            builder.Property(n => n.Type) .IsRequired().HasMaxLength(50);
            builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
            builder.Property(n => n.ReadAt).IsRequired(false);
            builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
            builder.Property(n => n.UserId).IsRequired();

            builder.HasOne(n => n.User).WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}
