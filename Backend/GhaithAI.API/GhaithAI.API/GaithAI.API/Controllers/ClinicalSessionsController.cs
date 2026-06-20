using GhaithAI.GaithAI.Application.DTOs.ClinicalSession;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            try
            {
                var sessionId = await _sessionService.StartSessionAsync(dto);
                return StatusCode(201, new { id = sessionId, message = "Session started successfully." });
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSession([FromRoute] Guid id)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(id);
                return Ok(session);
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

        [HttpPut("{id:guid}/end")]
        public async Task<IActionResult> EndSession([FromRoute] Guid id)
        {
            try
            {
                await _sessionService.EndSessionAsync(id);
                return Ok(new { message = "Session ended successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorSessions(
            [FromQuery] ClinicalSessionStatus? status,
            [FromQuery] DateTime? date)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            try
            {
                var sessions = await _sessionService.GetDoctorSessionsAsync(userId, status, date);
                return Ok(sessions);
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

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientSessions([FromRoute] string patientId)
        {
            try
            {
                var sessions = await _sessionService.GetPatientSessionsAsync(patientId);
                return Ok(sessions);
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
