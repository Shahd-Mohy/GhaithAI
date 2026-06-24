//namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
//{
//    public interface IStripeClient
//    {
//        /// <summary>
//        /// بيعمل Stripe Checkout Session وبيرجع الـ URL اللي المريض هيتحول عليه.
//        /// </summary>
//        Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
//            decimal amount,
//            string currency,
//            string bookingId,
//            string patientEmail,
//            string successUrl,
//            string cancelUrl);

//        /// <summary>
//        /// بيتحقق من توقيع الـ Webhook عشان يتأكد إن الـ request جاي من Stripe فعلاً.
//        /// بيرجع event type و paymentIntentId.
//        /// </summary>
//        Task<StripeWebhookResult> ValidateAndParseWebhookAsync(
//            string payload,
//            string stripeSignature);
//    }

//    public class StripeCheckoutResult
//    {
//        public string SessionId { get; set; }
//        public string CheckoutUrl { get; set; }
//    }

//    public class StripeWebhookResult
//    {
//        public string EventType { get; set; }
//        public string SessionId { get; set; }
//        public string? PaymentIntentId { get; set; }
//        public bool IsSuccess { get; set; }
//    }
//}