using GhaithAI.API.GaithAI.Application.DTOs.Report;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ILogger<SessionReportsController> _logger;

        public SessionReportsController(
            ISessionReportService reportService,
            ILogger<SessionReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // Generation

        /// <summary>
        /// Generate a clinical session report for a completed ClinicalSession.
        /// Calls the Langflow 3-stage pipeline and persists the result as Version 1.
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
                    nameof(GetReport),
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

        // Retrieval

        /// <summary>
        /// Get the current state of a session report by its ID.
        /// Returns the latest field values (SOAP, risk, chief complaint, etc.).
        /// </summary>
        [HttpGet("{reportId:guid}")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReport([FromRoute] Guid reportId)
        {
            try
            {
                var result = await _reportService.GetReportAsync(reportId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the session report linked to a specific ClinicalSession.
        /// Useful when the Frontend has a SessionId but not the ReportId.
        /// </summary>
        [HttpGet("by-session/{sessionId:guid}")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportBySession([FromRoute] Guid sessionId)
        {
            try
            {
                var result = await _reportService.GetReportBySessionAsync(sessionId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // Mutation

        /// <summary>
        /// Update the editable SOAP fields of a session report.
        /// Automatically saves a new version in history.
        /// Returns 409 Conflict if the report is Locked.
        /// </summary>
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
            try
            {
                var result = await _reportService.UpdateReportAsync(reportId, doctorId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve a Draft report (Draft → Approved).
        /// Returns 409 Conflict if the report is not in Draft status.
        /// </summary>
        [HttpPost("{reportId:guid}/approve")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ApproveReport(
            [FromRoute] Guid reportId,
            [FromQuery] Guid doctorId)
        {
            try
            {
                var result = await _reportService.ApproveReportAsync(reportId, doctorId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lock a report permanently (Draft/Approved → Locked).
        /// Once Locked, the report becomes read-only and cannot be edited.
        /// Returns 409 Conflict if the report is already Locked.
        /// </summary>
        [HttpPost("{reportId:guid}/lock")]
        [ProducesResponseType(typeof(SessionReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> LockReport(
            [FromRoute] Guid reportId,
            [FromQuery] Guid doctorId)
        {
            try
            {
                var result = await _reportService.LockReportAsync(reportId, doctorId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        // Versioning

        /// <summary>
        /// Get all versions of a session report, ordered newest-first.
        /// </summary>
        [HttpGet("{reportId:guid}/versions")]
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
        [HttpGet("{reportId:guid}/versions/{versionNumber:int?}")]
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
