using GhaithAI.API.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    /// <summary>
    /// Orchestrates the full session report lifecycle:
    ///   Generate → Edit (SOAP) → Approve → Lock.
    ///
    /// Business rules enforced here:
    ///   - Only Draft reports can be Approved.
    ///   - Locked reports cannot be edited or re-locked.
    ///   - Every successful edit creates a new <c>SessionReportVersion</c>.
    /// </summary>
    public interface ISessionReportService
    {
        // ─────────────────────────────────────────────────────────────────────
        // Generation
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Generate a clinical report for the specified ClinicalSession.
        /// Creates a <c>SessionReport</c> in Draft status with Version 1.
        /// </summary>
        Task<SessionReportResponseDto> GenerateReportAsync(
            Guid clinicalSessionId,
            Guid doctorId,
            string patientId);

        // ─────────────────────────────────────────────────────────────────────
        // Retrieval
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieve the current state of a report by its own ID.
        /// Throws <see cref="KeyNotFoundException"/> if not found.
        /// </summary>
        Task<SessionReportResponseDto> GetReportAsync(Guid reportId);

        /// <summary>
        /// Retrieve the report generated for a specific ClinicalSession.
        /// Throws <see cref="KeyNotFoundException"/> if no report exists for the session.
        /// </summary>
        Task<SessionReportResponseDto> GetReportBySessionAsync(Guid sessionId);

        // ─────────────────────────────────────────────────────────────────────
        // Mutation
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Update the editable SOAP fields of a report and persist a new version.
        /// Throws <see cref="InvalidOperationException"/> if the report is Locked.
        /// </summary>
        Task<SessionReportResponseDto> UpdateReportAsync(
            Guid reportId,
            Guid doctorId,
            UpdateReportDto dto);

        /// <summary>
        /// Approve a Draft report (Draft → Approved).
        /// Throws <see cref="InvalidOperationException"/> if the report is not in Draft status.
        /// </summary>
        Task<SessionReportResponseDto> ApproveReportAsync(Guid reportId, Guid doctorId);

        /// <summary>
        /// Lock a report permanently (Draft/Approved → Locked).
        /// Once Locked the report becomes read-only.
        /// Throws <see cref="InvalidOperationException"/> if the report is already Locked.
        /// </summary>
        Task<SessionReportResponseDto> LockReportAsync(Guid reportId, Guid doctorId);

        // ─────────────────────────────────────────────────────────────────────
        // Versioning
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieve all versions of a session report, ordered newest-first.
        /// </summary>
        Task<IEnumerable<SessionReportVersionDto>> GetVersionsAsync(Guid reportId);

        /// <summary>
        /// Retrieve a specific version of a session report.
        /// If <paramref name="versionNumber"/> is null, returns the latest version.
        /// </summary>
        Task<SessionReportVersionDto> GetVersionAsync(Guid reportId, int? versionNumber = null);
    }
}

