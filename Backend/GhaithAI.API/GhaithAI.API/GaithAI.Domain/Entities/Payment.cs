using GhaithAI.API.GaithAI.Domain.Common;

namespace GhaithAI.GaithAI.Domain.Entities
{
    public class Payment : BaseEntity<Guid>
    {
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }

        public string PatientId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Stripe Checkout Session ID
        public string StripeSessionId { get; set; }

        // Stripe Payment Intent ID — بييجي بعد الدفع الفعلي
        public string? StripePaymentIntentId { get; set; }

        public DateTime? PaidAt { get; set; }

        public string? FailureReason { get; set; }
    }
}
