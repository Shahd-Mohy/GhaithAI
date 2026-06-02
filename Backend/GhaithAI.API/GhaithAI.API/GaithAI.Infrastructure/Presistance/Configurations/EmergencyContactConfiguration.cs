using GhaithAI.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Data.Configurations
{
    /// <summary>
    /// Entity configuration for EmergencyContact entity.
    /// Configures the primary key, properties, and relationships for emergency contacts.
    /// </summary>
    public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
    {
        public void Configure(EntityTypeBuilder<EmergencyContact> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Relationship)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasOne(x => x.User)
                .WithMany(u => u.EmergencyContacts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
