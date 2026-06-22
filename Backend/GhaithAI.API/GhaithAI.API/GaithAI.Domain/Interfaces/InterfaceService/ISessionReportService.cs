using GhaithAI.API.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    /// <summary>
    /// Orchestrates the end-to-end session report lifecycle:
    ///   1. Fetches ordered <c>SessionTranscript</c> segments from the DB.
    ///   2. Formats them as a labelled plain-text transcript.
    ///   3. Calls the Langflow 3-stage clinical report pipeline.
    ///   4. Parses the returned JSON (SOAP + risk fields).
    ///   5. Persists a new <c>SessionReport</c> + <c>SessionReportVersion</c> (v1).
    /// </summary>
    public interface ISessionReportService
    {
        /// <summary>
        /// Generate a clinical report for the specified ClinicalSession.
        /// Creates a <c>SessionReport</c> in Draft status with Version 1.
        /// </summary>
        /// <param name="clinicalSessionId">The ID of the completed ClinicalSession.</param>
        /// <param name="doctorId">The ID of the clinician requesting the report.</param>
        /// <param name="patientId">The string ID of the patient.</param>
        /// <returns>The generated report details.</returns>
        Task<SessionReportResponseDto> GenerateReportAsync(
            Guid clinicalSessionId,
            Guid doctorId,
            string patientId);

        /// <summary>
        /// Retrieve all versions of a session report, ordered by version number.
        /// </summary>
        Task<IEnumerable<SessionReportVersionDto>> GetVersionsAsync(Guid reportId);

        /// <summary>
        /// Retrieve a specific version of a session report.
        /// </summary>
        Task<SessionReportVersionDto> GetVersionAsync(Guid reportId, int versionNumber);
    }
}
