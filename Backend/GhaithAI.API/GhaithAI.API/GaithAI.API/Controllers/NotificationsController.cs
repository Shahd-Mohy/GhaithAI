using GhaithAI.API.GaithAI.Application.DTOs.Notification;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<ActionResult<PagedNotificationsResponseDto>> GetMyNotifications([FromQuery] int page = 0, [FromQuery] int size = 10)
        {
            var result = await _notificationService.GetMyNotificationsAsync(UserId, page, size);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(UserId);
            return Ok(count);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id, UserId);
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(UserId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            await _notificationService.DeleteNotificationAsync(id, UserId);
            return NoContent();
        }

        [HttpPost("mock")]
        public async Task<IActionResult> TriggerMockNotifications()
        {
            var userId = UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            // 1. Session Report Ready
            await _notificationService.SendAsync(
                userId, 
                GhaithAI.GaithAI.Domain.Enums.NotificationType.SessionReportReady,
                "Session Report Ready 📄",
                "Your last session report has been generated successfully. You can review the insights and recommendations now."
            );

            // 2. Appointment Reminder
            await _notificationService.SendAsync(
                userId, 
                GhaithAI.GaithAI.Domain.Enums.NotificationType.AppointmentReminder,
                "Appointment Reminder ⏰",
                "Reminder: You have an upcoming support session in 24 hours with Dr. Ahmed El-Sherif."
            );

            // 3. Booking Confirmed
            await _notificationService.SendAsync(
                userId, 
                GhaithAI.GaithAI.Domain.Enums.NotificationType.BookingConfirmed,
                "Booking Confirmed ✅",
                "Congratulations! Your appointment booking has been approved and confirmed by the clinician."
            );

            return Ok(new { message = "Mock notifications sent successfully!" });
        }
    }
}
