using GhaithAI.GaithAI.Application.DTOs.Booking;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
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

            var bookingId = await _bookingService.CreateClinicBookingAsync(userId, dto);
            return StatusCode(201, new { id = bookingId, message = "booking successfully" });
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookDoctor([FromBody] CreateUserBookingDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var bookingId = await _bookingService.CreateUserBookingAsync(userId, dto);
            return StatusCode(201, new { id = bookingId, message = "Booking created successfully." });
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] string? timeFilter = "all",
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var bookings = await _bookingService.GetUserBookingsAsync(userId, timeFilter, pageIndex, pageSize);
            return Ok(bookings);
        }

        [HttpPost("{bookingId:guid}/cancel")]
        public async Task<IActionResult> CancelBooking([FromRoute] Guid bookingId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            await _bookingService.CancelUserBookingAsync(userId, bookingId);
            return Ok(new { message = "Booking cancelled successfully." });
        }

        [HttpGet("{doctorId:guid}/available-slots")]
        public async Task<IActionResult> GetAvailableSlots(
            [FromRoute] Guid doctorId,
            [FromQuery] DateTime date)
        {
            var slots = await _bookingService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetSchedule()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User is not authenticated." });

            var schedule = await _bookingService.GetTodaySchedulePagedAsync(userId);
            return Ok(schedule);
        }
    }
}
