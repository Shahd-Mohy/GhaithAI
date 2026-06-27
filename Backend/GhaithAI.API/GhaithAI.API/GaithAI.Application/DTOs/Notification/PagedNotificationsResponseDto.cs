using System.Collections.Generic;

namespace GhaithAI.API.GaithAI.Application.DTOs.Notification
{
    public class PagedNotificationsResponseDto
    {
        public IEnumerable<NotificationDto> Notifications { get; set; }
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }
}
