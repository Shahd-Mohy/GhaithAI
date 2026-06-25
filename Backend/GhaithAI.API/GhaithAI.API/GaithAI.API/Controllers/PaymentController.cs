using GhaithAI.GaithAI.Application.DTOs.Payment;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST api/payments/initiate
        [HttpPost("initiate")]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                var result = await _paymentService.InitiateAsync(dto.BookingId, patientId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // GET api/payments/confirm?session_id=cs_test_xxx
        // الـ Frontend بيكلمه بعد redirect من Stripe
        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
                return BadRequest(new { message = "session_id is required." });
            try
            {
                var result = await _paymentService.ConfirmPaymentAsync(session_id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET api/payments/status/{bookingId}
        [HttpGet("status/{bookingId}")]
        public async Task<IActionResult> GetStatus(Guid bookingId)
        {
            try
            {
                var result = await _paymentService.GetStatusByBookingIdAsync(bookingId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // POST api/payments/mock-confirm/{sessionId}  ← Testing فقط
        [HttpPost("mock-confirm/{sessionId}")]
        public async Task<IActionResult> MockConfirm(string sessionId)
        {
            try
            {
                var result = await _paymentService.MockConfirmAsync(sessionId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }
}