using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.Booking;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        [HttpGet("my-bookings-paged")]
        public async Task<IActionResult> GetMyBookingsPaged(
    [FromQuery] string? timeFilter = "all", 
    [FromQuery] int pageIndex = 0,
    [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var bookings = await _bookingService.GetDoctorBookingsPagedAsync(userId, timeFilter, pageIndex, pageSize);

            return Ok(bookings);
        }

        [HttpPost("create-clinic-booking")]
        public async Task<IActionResult> CreateClinicBooking([FromBody] CreateClinicBookingDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            try
            {
                var bookingId = await _bookingService.CreateClinicBookingAsync(userId, dto);

                return StatusCode(201, new { id = bookingId, message = "booking successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
