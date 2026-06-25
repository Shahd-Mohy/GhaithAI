using GhaithAI.API.Repositories.UnitWork;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GhaithAI.API.BackgroundJobs
{
    public class NotificationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationJob> _logger;

        public NotificationJob(IServiceProvider serviceProvider, ILogger<NotificationJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Job started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking and sending appointment reminders.");
                }

                // Wait 30 minutes before running again
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }

            _logger.LogInformation("Notification Job stopped.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            
            // Patient reminder: 24 hours before
            var tomorrowStart = now.AddHours(23.5);
            var tomorrowEnd = now.AddHours(24.5);

            var patientBookings = await unitOfWork.Booking.GetAllQueryableNoTracking()
                .Include(b => b.Doctor)
                .Where(b => b.Status != BookingStatus.Cancelled && b.PatientId != null)
                .ToListAsync();

            foreach (var booking in patientBookings)
            {
                var bookingDateTimeUtc = DateTime.SpecifyKind(booking.BookingDate.Add(booking.SlotTime), DateTimeKind.Utc);
                if (bookingDateTimeUtc >= tomorrowStart && bookingDateTimeUtc <= tomorrowEnd)
                {
                    // Check if notification already sent
                    var alreadySent = await unitOfWork.Notification.GetAllQueryableNoTracking()
                        .AnyAsync(n => n.TargetId == booking.Id && n.UserId == booking.PatientId && n.Type == NotificationType.AppointmentReminder);

                    if (!alreadySent)
                    {
                        await notificationService.SendAsync(
                            booking.PatientId,
                            NotificationType.AppointmentReminder,
                            "Appointment Reminder",
                            $"You have an appointment with Dr. {booking.Doctor.FullName} tomorrow at {booking.SlotTime}.",
                            booking.Id);
                    }
                }
            }

            // Doctor reminder: 1 hour before
            var nextHourStart = now.AddMinutes(30);
            var nextHourEnd = now.AddMinutes(90);

            var doctorBookings = await unitOfWork.Booking.GetAllQueryableNoTracking()
                .Include(b => b.Patient)
                .Where(b => b.Status != BookingStatus.Cancelled)
                .ToListAsync();

            foreach (var booking in doctorBookings)
            {
                var bookingDateTimeUtc = DateTime.SpecifyKind(booking.BookingDate.Add(booking.SlotTime), DateTimeKind.Utc);
                if (bookingDateTimeUtc >= nextHourStart && bookingDateTimeUtc <= nextHourEnd)
                {
                    // Find Doctor UserId
                    var doctorProfile = await unitOfWork.DoctorProfile.GetByIdAsync(booking.DoctorId);
                    if (doctorProfile != null)
                    {
                        var alreadySent = await unitOfWork.Notification.GetAllQueryableNoTracking()
                            .AnyAsync(n => n.TargetId == booking.Id && n.UserId == doctorProfile.UserId && n.Type == NotificationType.AppointmentReminder);

                        if (!alreadySent)
                        {
                            var patientName = booking.Patient?.FullName ?? "a patient";
                            await notificationService.SendAsync(
                                doctorProfile.UserId,
                                NotificationType.AppointmentReminder,
                                "Upcoming Appointment",
                                $"You have an appointment with {patientName} in an hour.",
                                booking.Id);
                        }
                    }
                }
            }
        }
    }
}
