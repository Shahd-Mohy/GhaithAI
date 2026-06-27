//using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
//using Microsoft.AspNetCore.Mvc;

//namespace GhaithAI.GaithAI.API.Controllers
//{
//    [ApiController]
//    [Route("api/webhooks")]
//    public class WebhookController : ControllerBase
//    {
//        private readonly IPaymentService _paymentService;

//        public WebhookController(IPaymentService paymentService)
//        {
//            _paymentService = paymentService;
//        }

//        [HttpPost("stripe")]
//        public async Task<IActionResult> StripeWebhook()
//        {
//            // ⚠️ لازم نقرا الـ raw body قبل أي middleware يعدله
//            var payload = await new StreamReader(HttpContext.Request.Body)
//                .ReadToEndAsync();

//            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

//            if (string.IsNullOrEmpty(stripeSignature))
//                return BadRequest(new { message = "Missing Stripe-Signature header." });

//            try
//            {
//                await _paymentService.HandleWebhookAsync(payload, stripeSignature);
//                return Ok(new { received = true });
//            }
//            catch (InvalidOperationException ex)
//            {
//                // Signature validation failed
//                return BadRequest(new { message = ex.Message });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Webhook processing error.", detail = ex.Message });
//            }
//        }
//    }
//}