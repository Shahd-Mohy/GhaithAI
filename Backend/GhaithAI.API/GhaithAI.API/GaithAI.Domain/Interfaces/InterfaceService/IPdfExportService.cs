namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateAsync(Guid report);
        Task<byte[]> ExportReportToPdfAsync(Guid reportId);
    }
}
