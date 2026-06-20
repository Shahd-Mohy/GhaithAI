using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [ApiController]
    [Authorize]
    public class ReportFeedbackController : ControllerBase
    {
        private readonly IReportFeedbackService _feedbackService;

        public ReportFeedbackController(IReportFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "Doctor";

        // POST /api/reports/{reportId}/feedback
        [HttpPost("api/reports/{reportId}/feedback")]
        [Authorize(Roles = "Clinician,Admin")]
        public async Task<IActionResult> AddTag(Guid reportId, [FromBody] CreateFeedbackTagDto dto)
        {
            var result = await _feedbackService.AddTagAsync(
                reportId, dto, CurrentUserId, CurrentUserRole);
            return Ok(result);
        }

        // GET /api/reports/{reportId}/feedback
        [HttpGet("api/reports/{reportId}/feedback")]
        [Authorize(Roles = "Clinician,Admin")]
        public async Task<IActionResult> GetByReport(Guid reportId)
        {
            var result = await _feedbackService.GetByReportIdAsync(reportId);
            return Ok(result);
        }

        // GET /api/admin/feedback-tags/analytics?tagType=&from=&to=
        [HttpGet("api/admin/feedback-tags/analytics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAnalytics(
            [FromQuery] string? tagType, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _feedbackService.GetAnalyticsAsync(tagType, from, to);
            return Ok(result);
        }
    }
}