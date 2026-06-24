using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.API.Presistance.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.StripeSessionId)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(p => p.StripePaymentIntentId)
                .HasMaxLength(500);

            builder.Property(p => p.PatientId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(p => p.FailureReason)
                .HasMaxLength(1000);

            builder.HasOne(p => p.Booking)
                .WithOne()
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.NoAction);

            // Stripe session id لازم يكون unique
            builder.HasIndex(p => p.StripeSessionId).IsUnique();
        }
    }
}