using GhaithAI.API.Responses;
using GhaithAI.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InsightController : ControllerBase
    {
        private readonly IInsightService _insightService;

        public InsightController(IInsightService insightService)
        {
            _insightService = insightService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        private string GetDisplayName() =>
            User.FindFirstValue(ClaimTypes.GivenName)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Friend";

        // GET /api/insight/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetUserId();
            var displayName = GetDisplayName();
            var result = await _insightService.GetDashboardAsync(userId, displayName);

            return Ok(SuccessResponse<DashboardViewModel>.Ok(result));
        }
    }
}
