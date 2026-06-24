using System.Security.Claims;
using GhaithAI.GaithAI.Application.DTOs.Payment;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.Controllers
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

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // POST /api/payments/initiate
        [HttpPost("initiate")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var result = await _paymentService.InitiateAsync(dto.BookingId, CurrentUserId);
            return Ok(result);
        }

        // POST /api/payments/mock-confirm
        // [AllowAnonymous] عشان الـ mock page مش عندها token
        [HttpPost("mock-confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> MockConfirm([FromBody] MockConfirmDto dto)
        {
            var result = await _paymentService.MockConfirmAsync(dto.SessionId);
            return Ok(result);
        }

        // GET /api/payments/{bookingId}/status
        [HttpGet("{bookingId}/status")]
        public async Task<IActionResult> GetStatus(Guid bookingId)
        {
            var result = await _paymentService.GetStatusByBookingIdAsync(bookingId);
            return Ok(result);
        }
    }

    public class MockConfirmDto
    {
        public string SessionId { get; set; }
    }
}