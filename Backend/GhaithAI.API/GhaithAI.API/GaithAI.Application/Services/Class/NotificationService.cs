using GhaithAI.API.GaithAI.Application.DTOs.Notification;
using GhaithAI.API.SignalR;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            IUnitOfWork unitOfWork, 
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task SendAsync(string userId, NotificationType type, string title, string body, Guid? targetId = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                IsRead = false,
                TargetId = targetId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notification.AddAsync(notification);
            await _unitOfWork.CompleteAsync();

            var dto = MapToDto(notification);
            
            // Send real-time notification
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", dto);
        }

        public async Task SendToAdminsAsync(NotificationType type, string title, string body, Guid? targetId = null)
        {
            // 1. Get all users in Admin role
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            // 2. Save notification to DB for each admin
            foreach (var admin in adminUsers)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = admin.Id,
                    Type = type,
                    Title = title,
                    Body = body,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    TargetId = targetId
                };

                await _unitOfWork.Notification.AddAsync(notification);
            }

            if (adminUsers.Any())
            {
                await _unitOfWork.CompleteAsync();
            }

            var dto = new NotificationDto
            {
                Id = Guid.NewGuid(),
                Title = title,
                Body = body,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                TargetId = targetId
            };

            await _hubContext.Clients.Group("Admin").SendAsync("ReceiveNotification", dto);
        }

        public async Task<PagedNotificationsResponseDto> GetMyNotificationsAsync(string userId, int page, int size)
        {
            var query = _unitOfWork.Notification.GetAllQueryableNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var unreadCount = await _unitOfWork.Notification.GetUnreadCountAsync(userId);

            var items = await query
                .Skip(page * size)
                .Take(size)
                .ToListAsync();

            return new PagedNotificationsResponseDto
            {
                Notifications = items.Select(MapToDto),
                TotalCount = totalCount,
                UnreadCount = unreadCount
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.Notification.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            await _unitOfWork.Notification.MarkAsReadAsync(notificationId, userId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _unitOfWork.Notification.MarkAllAsReadAsync(userId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteNotificationAsync(Guid notificationId, string userId)
        {
            var notification = await _unitOfWork.Notification.GetAllQueryableTracking()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                _unitOfWork.Notification.Delete(notification);
                await _unitOfWork.CompleteAsync();
            }
        }

        private NotificationDto MapToDto(Notification n) => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            TargetId = n.TargetId
        };
    }
}
