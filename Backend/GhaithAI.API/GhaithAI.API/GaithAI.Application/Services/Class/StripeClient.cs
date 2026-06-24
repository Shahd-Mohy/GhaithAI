//using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
//using Microsoft.Extensions.Logging;
//using Stripe;
//using Stripe.Checkout;

//namespace GhaithAI.GaithAI.Application.Services.Class
//{
//    public class StripeClient : IStripeClient
//    {
//        private readonly IConfiguration _configuration;

//        public StripeClient(IConfiguration configuration)
//        {
//            _configuration = configuration;
//            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
//        }

//        public async Task<StripeCheckoutResult> CreateCheckoutSessionAsync(
//            decimal amount,
//            string currency,
//            string bookingId,
//            string patientEmail,
//            string successUrl,
//            string cancelUrl)
//        {
//            var options = new SessionCreateOptions
//            {
//                PaymentMethodTypes = new List<string> { "card" },
//                LineItems = new List<SessionLineItemOptions>
//                {
//                    new SessionLineItemOptions
//                    {
//                        PriceData = new SessionLineItemPriceDataOptions
//                        {
//                            Currency = currency,
//                            UnitAmount = (long)(amount * 100), // Stripe بياخد cents
//                            ProductData = new SessionLineItemPriceDataProductDataOptions
//                            {
//                                Name = "GhaithAI Therapy Session",
//                                Description = $"Booking #{bookingId}"
//                            }
//                        },
//                        Quantity = 1
//                    }
//                },
//                Mode = "payment",
//                CustomerEmail = patientEmail,
//                SuccessUrl = $"{successUrl}?session_id={{CHECKOUT_SESSION_ID}}",
//                CancelUrl = cancelUrl,
//                Metadata = new Dictionary<string, string>
//                {
//                    { "bookingId", bookingId }
//                }
//            };

//            var service = new SessionService();
//            var session = await service.CreateAsync(options);

//            return new StripeCheckoutResult
//            {
//                SessionId = session.Id,
//                CheckoutUrl = session.Url
//            };
//        }

//        public async Task<StripeWebhookResult> ValidateAndParseWebhookAsync(
//            string payload,
//            string stripeSignature)
//        {
//            var webhookSecret = _configuration["Stripe:WebhookSecret"];

//            try
//            {
//                var stripeEvent = EventUtility.ConstructEvent(
//                    payload,
//                    stripeSignature,
//                    webhookSecret);

//                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
//                {
//                    var session = stripeEvent.Data.Object as Session;
//                    return new StripeWebhookResult
//                    {
//                        EventType = stripeEvent.Type,
//                        SessionId = session?.Id ?? "",
//                        PaymentIntentId = session?.PaymentIntentId,
//                        IsSuccess = true
//                    };
//                }

//                if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
//                {
//                    var intent = stripeEvent.Data.Object as PaymentIntent;
//                    return new StripeWebhookResult
//                    {
//                        EventType = stripeEvent.Type,
//                        SessionId = "",
//                        PaymentIntentId = intent?.Id,
//                        IsSuccess = false
//                    };
//                }

//                return new StripeWebhookResult
//                {
//                    EventType = stripeEvent.Type,
//                    IsSuccess = false
//                };
//            }
//            catch (StripeException ex)
//            {
//                throw new Exception($"Webhook validation failed: {ex.Message}");
//            }
//        }
//    }
//}