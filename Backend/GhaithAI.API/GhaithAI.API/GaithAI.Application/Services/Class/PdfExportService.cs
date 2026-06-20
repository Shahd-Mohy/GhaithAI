using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class PdfExportService : IPdfExportService
    {
        public Task<string> GenerateAsync(ClinicalReport report)
        {
            throw new NotImplementedException("PDF export will be implemented in the next sprint.");
        }

        public Task<byte[]> ExportReportToPdfAsync(Guid reportId)
        {
            throw new NotImplementedException("PDF export will be implemented in the next sprint.");
        }
    }
}
