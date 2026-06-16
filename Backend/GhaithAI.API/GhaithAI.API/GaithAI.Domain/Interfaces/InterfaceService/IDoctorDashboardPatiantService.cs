using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IDoctorDashboardPatiantService
    {
        Task<DoctorPatientsDashboardDto> GetDoctorPatientsDashboardAsync(
            string userId, 
            string? search = null,
            string? riskFilter = null,
            int pageNumber = 1,
            int pageSize = 10);
    }
}
