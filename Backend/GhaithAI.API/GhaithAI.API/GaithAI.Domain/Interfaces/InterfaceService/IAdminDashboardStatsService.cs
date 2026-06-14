using GhaithAI.GaithAI.Application.DTOs.Admin;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IAdminDashboardStatsService
    {
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
    }
}
