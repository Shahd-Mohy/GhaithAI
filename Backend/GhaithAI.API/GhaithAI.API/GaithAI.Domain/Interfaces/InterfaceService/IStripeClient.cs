namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IStripeClient
    {
        Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string bookingId,
            string patientEmail,
            string successUrl,
            string cancelUrl);

        Task<StripeWebhookResult> ValidateAndParseWebhookAsync(
            string payload,
            string stripeSignature);
    }

    public class StripeCheckoutResult
    {
        public string SessionId { get; set; }
        public string CheckoutUrl { get; set; }
    }

    public class StripeWebhookResult
    {
        public string EventType { get; set; }
        public string SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
        public bool IsSuccess { get; set; }
    }
}