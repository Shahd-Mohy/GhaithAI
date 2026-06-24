namespace GhaithAI.GaithAI.Application.DTOs.Payment
{
    public class PaymentStatusDto
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
