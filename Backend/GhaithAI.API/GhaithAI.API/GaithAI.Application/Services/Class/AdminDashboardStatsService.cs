using System;
using System.Linq;
using System.Threading.Tasks;
using GhaithAI.GaithAI.Application.DTOs.Admin;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class AdminDashboardStatsService : IAdminDashboardStatsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminDashboardStatsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var doctors = await _unitOfWork.DoctorProfile.GetAllAsync();
                var bookings = await _unitOfWork.Booking.GetAllAsync(); 
                var selfHelps = await _unitOfWork.SelfHelp.GetAllAsync();
                var sessions = await _unitOfWork.Session.GetAllAsync();
                var messages = await _unitOfWork.Message.GetAllAsync();

                var pendingDoctors = doctors.Count(d => d.ApprovalStatus == ApprovalStatus.Pending && !d.IsDeleted);
                var approvedDoctors = doctors.Count(d => d.ApprovalStatus == ApprovalStatus.Approved && !d.IsDeleted);
                var totalBookings = bookings.Count();
                var totalSelfHelp = selfHelps.Count(s => !s.IsDeleted);
                var totalChats = sessions.Count();
                var totalMessages = messages.Count();


                return new AdminDashboardStatsDto
                {
                    PendingDoctors = pendingDoctors,
                    ApprovedDoctors = approvedDoctors,
                    TotalBookings = totalBookings,
                    TotalSelfHelpContents = totalSelfHelp,
                    TotalAIChatSessions = totalChats,
                    TotalAIChatMassage = totalMessages 
                };
            }
            catch (Exception ex)
            {
                throw new ApplicationException("A system error occurred while generating dashboard metrics.", ex);
            }
        }
    }
}