using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.API.GaithAI.Application.Services.Class;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace GhaithAI.API.GaithAI.API.Controllers
{
    /// <summary>
    /// Manages the full lifecycle of a clinical session report:
    ///   Generate → Get → Edit (SOAP) → Approve → Lock.
    /// All endpoints require authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionReportsController : ControllerBase
    {
        private readonly ISessionReportService _reportService;
        private readonly ISessionReportPdfService _pdfService;
        private readonly IReportLogoProvider _logoProvider;
        private readonly ILogger<SessionReportsController> _logger;

        public SessionReportsController(
            ISessionReportService reportService,
            ISessionReportPdfService pdfService,
            IReportLogoProvider logoProvider,
            ILogger<SessionReportsController> logger)
        {
            _reportService = reportService;
            _pdfService = pdfService;
            _logoProvider = logoProvider;
            _logger = logger;
        }

        [HttpPost("{sessionId}/generate")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateReport(
            [FromRoute] Guid sessionId,
            [FromQuery] Guid doctorId,
            [FromQuery] string patientId)
        {
            var result = await _reportService.GenerateReportAsync(sessionId, doctorId, patientId);
            return CreatedAtAction(nameof(GetReport), new { reportId = result.ReportId }, result);
        }

        [HttpGet("{reportId:guid}")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReport([FromRoute] Guid reportId)
        {
            var result = await _reportService.GetReportAsync(reportId);
            return Ok(result);
        }

        [HttpGet("by-session/{sessionId:guid}")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportBySession([FromRoute] Guid sessionId)
        {
            var result = await _reportService.GetReportBySessionAsync(sessionId);
            return Ok(result);
        }

        [HttpGet("{reportId:guid}/export-pdf")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportPdf([FromRoute] Guid reportId)
        {
            var pdfBytes = await _pdfService.GeneratePdfAsync(reportId);
            var fileName = $"GhaithAI_Report_{reportId:N}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpPost("preview-pdf")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public IActionResult PreviewPdf([FromBody] PdfReportDataDto dto)
        {
            var document = new SessionReportPdfDocument(dto, _logoProvider.LogoBytes);
            var pdfBytes = document.GeneratePdf();
            var fileName = "GhaithAI_Report_Preview.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpPut("{reportId:guid}")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateReport(
            [FromRoute] Guid reportId,
            [FromQuery] Guid doctorId,
            [FromBody] UpdateReportDto dto)
        {
            var result = await _reportService.UpdateReportAsync(reportId, doctorId, dto);
            return Ok(result);
        }

        [HttpPost("{reportId:guid}/approve")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ApproveReport(
            [FromRoute] Guid reportId,
            [FromQuery] Guid doctorId)
        {
            var result = await _reportService.ApproveReportAsync(reportId, doctorId);
            return Ok(result);
        }

        [HttpPost("{reportId:guid}/lock")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> LockReport(
            [FromRoute] Guid reportId,
            [FromQuery] Guid doctorId)
        {
            var result = await _reportService.LockReportAsync(reportId, doctorId);
            return Ok(result);
        }

        [HttpGet("{reportId:guid}/versions")]
        [ProducesResponseType(typeof(IEnumerable<SessionReportVersionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVersions([FromRoute] Guid reportId)
        {
            var versions = await _reportService.GetVersionsAsync(reportId);
            return Ok(versions);
        }

        [HttpGet("{reportId:guid}/versions/{versionNumber:int?}")]
        [ProducesResponseType(typeof(SessionReportVersionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVersion(
            [FromRoute] Guid reportId,
            [FromRoute] int? versionNumber = null)
        {
            var version = await _reportService.GetVersionAsync(reportId, versionNumber);
            return Ok(version);
        }
    }
}
