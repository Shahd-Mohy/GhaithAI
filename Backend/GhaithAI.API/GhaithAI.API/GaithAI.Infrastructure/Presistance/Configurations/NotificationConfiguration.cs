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
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Body).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
            builder.Property(n => n.ReadAt).IsRequired(false);
                 
            builder.HasOne(n => n.User).WithMany()
                .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
