using GhaithAI.API.GaithAI.Application.DTOs.Mail;
using GhaithAI.API.GaithAI.Application.DTOs.Notification;
using GhaithAI.API.SignalR;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MimeKit;

namespace GhaithAI.API.GaithAI.Application.Services.Class
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly IWebHostEnvironment _env;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager,
            IMailService mailService,
            IConfiguration configuration,
            ILogger<NotificationService> logger,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _userManager = userManager;
            _mailService = mailService;
            _configuration = configuration;
            _logger = logger;
            _env = env;
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

            // Also send email to user (doctor) if email exists
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var email = user?.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var frontend = _configuration["FrontendUrl"]?.TrimEnd('/') ?? string.Empty;
                    var backend = _configuration["BackendUrl"]?.TrimEnd('/') ?? string.Empty;

                    // Use PNG logo via CID (inline image)
                    string logoSrc = "cid:logo1.png"; // PNG file placed in wwwroot/assets/logo1.png

                    // Determine button text and target link dynamically based on notification type and roles
                    string buttonText = "Go to Dashboard";
                    string targetLink = string.Empty;

                    bool isClinician = user != null && await _userManager.IsInRoleAsync(user, "Clinician");
                    bool isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

                    if (targetId.HasValue && !string.IsNullOrWhiteSpace(frontend))
                    {
                        switch (type)
                        {
                            case NotificationType.BookingConfirmed:
                                buttonText = "View Dashboard";
                                targetLink = isClinician ? $"{frontend}/clinician-dashboard" : $"{frontend}/dashboard";
                                break;
                            case NotificationType.BookingCancelled:
                            case NotificationType.BookingCancelledByDoctor:
                            case NotificationType.BookingCancelledByPatient:
                                buttonText = "Go to Dashboard";
                                targetLink = isClinician ? $"{frontend}/clinician-dashboard" : $"{frontend}/dashboard";
                                break;
                            case NotificationType.AppointmentReminder:
                                buttonText = "View Session Room";
                                targetLink = $"{frontend}/session-room/{targetId.Value}";
                                break;
                            case NotificationType.SessionStarted:
                                buttonText = "Join Session";
                                targetLink = $"{frontend}/session-room/{targetId.Value}";
                                break;
                            case NotificationType.SessionReportReady:
                                buttonText = "View Report";
                                targetLink = isClinician
                                    ? $"{frontend}/clinical-session/{targetId.Value}/report"
                                    : $"{frontend}/session/{targetId.Value}/report";
                                break;
                            case NotificationType.NewBooking:
                                buttonText = "Manage Bookings";
                                targetLink = $"{frontend}/clinician-dashboard";
                                break;
                            case NotificationType.ProfileApproved:
                            case NotificationType.ProfileRejected:
                                buttonText = "Go to Profile";
                                targetLink = isClinician ? $"{frontend}/clinician-dashboard" : $"{frontend}/dashboard";
                                break;
                            case NotificationType.NewDoctorRegistration:
                                buttonText = "Review Profile";
                                targetLink = $"{frontend}/admin/doctor/{targetId.Value}";
                                break;
                            case NotificationType.NewReportFlagged:
                                buttonText = "Review Report";
                                targetLink = $"{frontend}/admin";
                                break;
                            default:
                                buttonText = "Go to Dashboard";
                                targetLink = isClinician ? $"{frontend}/clinician-dashboard" : $"{frontend}/dashboard";
                                break;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(frontend))
                    {
                        targetLink = isClinician ? $"{frontend}/clinician-dashboard" : $"{frontend}/dashboard";
                        buttonText = "Go to Dashboard";
                    }

                    var html = $@"
                    <div style='font-family: Arial, Helvetica, sans-serif; max-width:600px; margin:0 auto; color:#0D1B3E;'>
                      <div style='text-align:center; padding:24px 0;'>
                        <img src='{logoSrc}' alt='GhaithAI' style='width:120px; height:auto;' />
                      </div>
                      <div style='background:#ffffff;border:1px solid #E6EEF2;border-radius:8px;padding:24px;'>
                        <h2 style='margin:0 0 12px; color:#0B8FAC;'>{title}</h2>
                        <p style='color:#475569;line-height:1.6'>{body}</p>
                        {(string.IsNullOrWhiteSpace(targetLink) ? string.Empty : $"<div style='text-align:center;margin:20px 0;'><a href='{targetLink}' style='background:#0B8FAC;color:#fff;padding:12px 20px;border-radius:6px;text-decoration:none;font-weight:600;'>{buttonText}</a></div>")}
                        <p style='font-size:12px;color:#94A3B8;margin:16px 0 0;'>This is an automated message from GhaithAI.</p>
                      </div>
                    </div>";

                    var mail = new MailRequest
                    {
                        ToEmail = email,
                        Subject = title,
                        Body = html,
                        LinkedResources = new List<MimeEntity>
                        {
                            new MimePart("image", "png")
                            {
                                ContentId = "logo1.png",
                                // Load the PNG from wwwroot/assets/logo1.png
                                Content = new MimeContent(File.OpenRead(Path.Combine(_env.WebRootPath ?? "wwwroot", "assets", "logo1.png")), ContentEncoding.Default),
                                ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                                ContentTransferEncoding = ContentEncoding.Base64
                            }
                        }
                    };

                    await _mailService.SendEmailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                // Log and continue - the notification has already been persisted and sent via SignalR
                _logger?.LogWarning(ex, "Failed to send notification email to user {UserId}", userId);
            }
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
