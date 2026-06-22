namespace GhaithAI.API.GaithAI.Application.DTOs.Report
{
    /// <summary>
    /// Result returned from <c>IReportLangflowService</c> after a successful
    /// Langflow pipeline call.
    /// </summary>
    /// <param name="ParsedOutput">Deserialized SOAP + risk data.</param>
    /// <param name="RawJson">The original raw JSON string for audit/versioning.</param>
    public sealed record ReportLangflowResult(
        ReportAiOutputDto ParsedOutput,
        string RawJson);
}
