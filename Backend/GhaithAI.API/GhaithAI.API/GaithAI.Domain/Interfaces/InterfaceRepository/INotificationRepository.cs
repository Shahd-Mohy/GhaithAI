using GhaithAI.GaithAI.Domain.Entities;
using System.Threading.Tasks;
using System;

namespace GhaithAI.API.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
