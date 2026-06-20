using GhaithAI.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [ApiController]
    [Authorize]
    public class ClinicalReportsController : ControllerBase
    {
        private readonly IReportGenerationService _reportGenerationService;
        private readonly IReportApprovalService _reportApprovalService;
        private readonly IReportHistoryService _reportHistoryService;

        public ClinicalReportsController(
            IReportGenerationService reportGenerationService,
            IReportApprovalService reportApprovalService,
            IReportHistoryService reportHistoryService)
        {
            _reportGenerationService = reportGenerationService;
            _reportApprovalService = reportApprovalService;
            _reportHistoryService = reportHistoryService;
        }

        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // POST /api/sessions/{sessionId}/reports/generate
        [HttpPost("api/sessions/{sessionId}/reports/generate")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> Generate(Guid sessionId)
        {
            var result = await _reportGenerationService.GenerateAsync(sessionId, CurrentUserId);
            return Ok(result);
        }

        // GET /api/reports/{reportId}
        [HttpGet("api/reports/{reportId}")]
        public async Task<IActionResult> GetById(Guid reportId)
        {
            var result = await _reportGenerationService.GetByIdAsync(reportId);
            return Ok(result);
        }

        // PUT /api/reports/{reportId}/sections/{sectionId}
        [HttpPut("api/reports/{reportId}/sections/{sectionId}")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> UpdateSection(
            Guid reportId, Guid sectionId, [FromBody] UpdateReportSectionDto dto)
        {
            var result = await _reportApprovalService.UpdateSectionAsync(
                reportId, sectionId, dto, CurrentUserId);
            return Ok(result);
        }

        // PUT /api/reports/{reportId}/notes
        [HttpPut("api/reports/{reportId}/notes")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> UpdateDoctorNotes(
            Guid reportId, [FromBody] UpdateDoctorNotesDto dto)
        {
            var result = await _reportApprovalService.UpdateDoctorNotesAsync(
                reportId, dto, CurrentUserId);
            return Ok(result);
        }

        // POST /api/reports/{reportId}/approve
        [HttpPost("api/reports/{reportId}/approve")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> Approve(
            Guid reportId, [FromBody] ApproveReportDto dto)
        {
            var result = await _reportApprovalService.ApproveAsync(reportId, dto, CurrentUserId);
            return Ok(result);
        }

        // GET /api/reports/{reportId}/export?format=pdf
        [HttpGet("api/reports/{reportId}/export")]
        public async Task<IActionResult> Export(Guid reportId, [FromQuery] string format = "pdf")
        {
            var pdfUrl = await _reportApprovalService.ExportPdfAsync(reportId);
            return Ok(new { pdfUrl });
        }

        // GET /api/reports/{reportId}/history
        [HttpGet("api/reports/{reportId}/history")]
        [Authorize(Roles = "Clinician,Admin")]
        public async Task<IActionResult> GetHistory(Guid reportId)
        {
            var result = await _reportHistoryService.GetByReportIdAsync(reportId);
            return Ok(result);
        }

        // GET /api/patient/sessions/{sessionId}/report
        [HttpGet("api/patient/sessions/{sessionId}/report")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetPatientReport(Guid sessionId)
        {
            var result = await _reportGenerationService.GetApprovedByPatientSessionAsync(
                sessionId, CurrentUserId);

            if (result == null)
                return NotFound("Report not available yet.");

            return Ok(result);
        }
    }
}