using GhaithAI.GaithAI.Application.DTOs.ClinicalSession;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    [Authorize]
    public class ClinicalSessionsController : ControllerBase
    {
        private readonly IClinicalSessionService _sessionService;

        public ClinicalSessionsController(IClinicalSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
        {
            var sessionId = await _sessionService.StartSessionAsync(dto);
            return StatusCode(201, new { id = sessionId, message = "Session started successfully." });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSession([FromRoute] Guid id)
        {
            var session = await _sessionService.GetSessionByIdAsync(id);
            return Ok(session);
        }

        [HttpGet("by-booking/{bookingId:guid}")]
        public async Task<IActionResult> GetSessionByBookingId([FromRoute] Guid bookingId)
        {
            var session = await _sessionService.GetSessionByBookingIdAsync(bookingId);
            return Ok(session);
        }

        [HttpPut("{id:guid}/end")]
        public async Task<IActionResult> EndSession([FromRoute] Guid id)
        {
            await _sessionService.EndSessionAsync(id);
            return Ok(new { message = "Session ended successfully." });
        }

        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorSessions(
            [FromQuery] ClinicalSessionStatus? status,
            [FromQuery] DateTime? date)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var sessions = await _sessionService.GetDoctorSessionsAsync(userId, status, date);
            return Ok(sessions);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientSessions([FromRoute] string patientId)
        {
            var sessions = await _sessionService.GetPatientSessionsAsync(patientId);
            return Ok(sessions);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
