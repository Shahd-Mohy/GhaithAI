using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorDashboardPatiantController : ControllerBase
    {
        private readonly IDoctorDashboardPatiantService _doctorService;
        private readonly ILogger<DoctorDashboardPatiantController> _logger;

        public DoctorDashboardPatiantController(
            IDoctorDashboardPatiantService doctorDashboardPatiantService,
            ILogger<DoctorDashboardPatiantController> logger)
        {
            _doctorService = doctorDashboardPatiantService;
            _logger = logger;
        }

        [HttpGet("patients-dashboard")]
        public async Task<IActionResult> GetPatientsDashboard(
            [FromQuery] string? search,
            [FromQuery] string? riskFilter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt to DoctorDashboard.");
                return Unauthorized(new { message = "Authentication failed: User ID not found." });
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            if (!string.IsNullOrEmpty(riskFilter))
            {
                var validRisks = new[] { "high", "medium", "low" };
                if (!validRisks.Contains(riskFilter.ToLower()))
                {
                    _logger.LogWarning("Invalid riskFilter received: {RiskFilter}", riskFilter);
                    return BadRequest(new { message = "Invalid risk level. Allowed values: high, medium, low." });
                }
            }

            _logger.LogInformation("Fetching patient dashboard for User: {UserId}", userId);
            var result = await _doctorService.GetDoctorPatientsDashboardAsync(userId, search, riskFilter, pageNumber, pageSize);
            return Ok(result);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}