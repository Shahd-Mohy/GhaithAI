using GhaithAI.API.GaithAI.Application.DTOs.Report;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    /// <summary>
    /// Responsible exclusively for the HTTP communication with the Langflow
    /// 3-stage clinical report pipeline.
    ///
    /// Contract:
    ///   • Input  — a plain-text session transcript and the session ID (for logging).
    ///   • Output — <see cref="ReportLangflowResult"/> containing both the parsed
    ///              <see cref="ReportAiOutputDto"/> and the original raw JSON string.
    ///
    /// This service has no knowledge of the database or domain persistence.
    /// </summary>
    public interface IReportLangflowService
    {
        /// <summary>
        /// Sends <paramref name="transcriptText"/> to the Langflow report pipeline
        /// and returns the structured AI output together with the raw JSON for storage.
        /// </summary>
        /// <param name="transcriptText">
        ///   Pre-formatted transcript (e.g. "[Doctor:] …\n[Patient:] …").
        /// </param>
        /// <param name="sessionId">Used as the Langflow session_id and for logging.</param>
        Task<ReportLangflowResult> AnalyzeTranscriptAsync(
            string transcriptText,
            Guid sessionId);
    }
}
