using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public WebhookController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        //[HttpPost("stripe")]
        //public async Task<IActionResult> StripeWebhook()
        //{
        //    // ✅ Stripe بيبعت raw body - لازم نقراه كده
        //    var payload = await new StreamReader(HttpContext.Request.Body)
        //        .ReadToEndAsync();

        //    var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        //    if (string.IsNullOrEmpty(stripeSignature))
        //        return BadRequest(new { message = "Missing Stripe signature." });

        //    try
        //    {
        //        await _paymentService.HandleWebhookAsync(payload, stripeSignature);
        //        return Ok();
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}