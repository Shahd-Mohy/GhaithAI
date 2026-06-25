using GhaithAI.API.GaithAI.Application.DTOs.Notification;
using GhaithAI.GaithAI.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface INotificationService
    {
        Task SendAsync(string userId, NotificationType type, string title, string body, Guid? targetId = null);
        Task SendToAdminsAsync(NotificationType type, string title, string body, Guid? targetId = null);
        Task<PagedNotificationsResponseDto> GetMyNotificationsAsync(string userId, int page, int size);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteNotificationAsync(Guid notificationId, string userId);
    }
}
