using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.GaithAI.API.Controllers
{
    /// <summary>
    /// Handles session report generation and versioned retrieval.
    /// All endpoints require authentication.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionReportsController : ControllerBase
    {
        private readonly ISessionReportService _reportService;
        private readonly ILogger<SessionReportsController> _logger;

        public SessionReportsController(
            ISessionReportService reportService,
            ILogger<SessionReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a clinical session report for a completed ClinicalSession.
        /// Calls the Langflow 3-stage pipeline and persists the result.
        /// </summary>
        /// <param name="sessionId">The ClinicalSession ID.</param>
        /// <param name="doctorId">The clinician's DoctorProfile ID.</param>
        /// <param name="patientId">The patient's ApplicationUser ID.</param>
        [HttpPost("{sessionId}/generate")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateReport(
            [FromRoute] Guid sessionId,
            [FromQuery] Guid doctorId,
            [FromQuery] string patientId)
        {
            try
            {
                var result = await _reportService.GenerateReportAsync(
                    sessionId, doctorId, patientId);

                return CreatedAtAction(
                    nameof(GetVersions),
                    new { reportId = result.ReportId },
                    result);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("No transcript"))
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all versions of a session report, ordered by version number.
        /// </summary>
        [HttpGet("{reportId}/versions")]
        [ProducesResponseType(typeof(IEnumerable<SessionReportVersionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVersions([FromRoute] Guid reportId)
        {
            var versions = await _reportService.GetVersionsAsync(reportId);
            return Ok(versions);
        }

        /// <summary>
        /// Get a specific version of a session report.
        /// If the version number is omitted, returns the latest version.
        /// </summary>
        [HttpGet("{reportId}/versions/{versionNumber:int?}")]
        [ProducesResponseType(typeof(SessionReportVersionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVersion(
            [FromRoute] Guid reportId,
            [FromRoute] int? versionNumber = null)
        {
            try
            {
                var version = await _reportService.GetVersionAsync(reportId, versionNumber);
                return Ok(version);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
