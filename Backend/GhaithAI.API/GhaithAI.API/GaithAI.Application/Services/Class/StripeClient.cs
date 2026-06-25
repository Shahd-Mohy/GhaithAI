using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using IStripeClientContract = GhaithAI.GaithAI.Domain.Interfaces.InterfaceService.IStripeClient; // ← حل الـ conflict
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace GhaithAI.GaithAI.Infrastructure.Services
{
    public class StripeClient : IStripeClientContract
    {
        public StripeClient(IConfiguration config)
        {
            var secretKey = config["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe:SecretKey is missing.");

            StripeConfiguration.ApiKey = secretKey;
            // ← مفيش WebhookSecret خالص
        }

        public async Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
            decimal amount,
            string currency,
            string bookingId,
            string patientEmail,
            string successUrl,
            string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = patientEmail,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency   = currency.ToLower(),
                            UnitAmount = (long)(amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name        = "Booking Payment",
                                Description = $"Payment for Booking #{bookingId}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "bookingId", bookingId }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new StripeCheckoutResult
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }

        // ← شيل ValidateAndParseWebhookAsync كاملة أو خليها throw
        public Task<StripeWebhookResult> ValidateAndParseWebhookAsync(
            string payload, string stripeSignature)
            => throw new NotSupportedException("Webhook not used in this mode.");
    }
}