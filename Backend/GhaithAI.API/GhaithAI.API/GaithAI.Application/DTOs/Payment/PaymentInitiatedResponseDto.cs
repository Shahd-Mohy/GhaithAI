namespace GhaithAI.GaithAI.Application.DTOs.Payment
{
    public class PaymentInitiatedResponseDto
    {
        public Guid PaymentId { get; set; }
        public string CheckoutUrl { get; set; }  // رابط Stripe Checkout
        public string StripeSessionId { get; set; }
    }
}
