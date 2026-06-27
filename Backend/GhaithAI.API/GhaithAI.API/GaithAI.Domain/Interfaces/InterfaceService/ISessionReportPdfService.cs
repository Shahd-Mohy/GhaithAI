namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    /// <summary>
    /// Generates a fully-formatted PDF byte array for a clinical session report.
    /// </summary>
    public interface ISessionReportPdfService
    {
        /// <summary>
        /// Loads all required data for the report and renders it as a PDF.
        /// </summary>
        /// <param name="reportId">The SessionReport ID.</param>
        /// <returns>PDF file bytes ready to stream to the caller.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the report is not found.</exception>
        Task<byte[]> GeneratePdfAsync(Guid reportId);
    }
}
