using GhaithAI.GaithAI.Domain.Enums;
using System;

namespace GhaithAI.API.GaithAI.Application.DTOs.Notification
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? TargetId { get; set; }
    }
}
