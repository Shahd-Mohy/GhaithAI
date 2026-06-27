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

        [HttpPost("initiate")]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _paymentService.InitiateAsync(dto.BookingId, patientId);
            return Ok(result);
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
                return BadRequest(new { message = "session_id is required." });

            var result = await _paymentService.ConfirmPaymentAsync(session_id);
            return Ok(result);
        }

        [HttpGet("status/{bookingId}")]
        public async Task<IActionResult> GetStatus(Guid bookingId)
        {
            var result = await _paymentService.GetStatusByBookingIdAsync(bookingId);
            return Ok(result);
        }

        [HttpPost("mock-confirm/{sessionId}")]
        public async Task<IActionResult> MockConfirm(string sessionId)
        {
            var result = await _paymentService.MockConfirmAsync(sessionId);
            return Ok(result);
        }
    }
}