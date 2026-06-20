namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IPdfExportService
    {
        Task<string> GenerateAsync(ClinicalReport report);
        Task<byte[]> ExportReportToPdfAsync(Guid reportId);
    }
}
