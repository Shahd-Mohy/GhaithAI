using GhaithAI.API.ViewModels;

namespace GhaithAI.API.Services.Interfaces
{
    public interface IInsightService
    {
        Task<DashboardViewModel> GetDashboardAsync(string userId, string displayName);
    }
}
