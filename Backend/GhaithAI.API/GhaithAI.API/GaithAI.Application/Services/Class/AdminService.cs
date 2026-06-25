using GhaithAI.GaithAI.Application.DTOs.Admin;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<DoctorProfileDTO>> GetPendingDoctorsAsync()
        {
            var doctors = await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x => x.ApprovalStatus == ApprovalStatus.Pending)
                .ToListAsync();

            return doctors.Select(MapToDTO).ToList();
        }

        public async Task<List<DoctorProfileDTO>> GetApprovedDoctorsAsync()
        {
            var doctors = await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x => x.ApprovalStatus == ApprovalStatus.Approved)
                .ToListAsync();

            return doctors.Select(MapToDTO).ToList();
        }

        public async Task<List<DoctorProfileDTO>> GetRejectedDoctorsAsync()
        {
            var doctors = await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x => x.ApprovalStatus == ApprovalStatus.Rejected)
                .ToListAsync();

            return doctors.Select(MapToDTO).ToList();
        }

        public async Task<DoctorProfileDTO> GetDoctorDetailsAsync(Guid id)
        {
            var doctor = await _context.DoctorsProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            return MapToDTO(doctor);
        }

        public async Task ApproveDoctorAsync(Guid id)
        {
            var doctor = await _context.DoctorsProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            doctor.ApprovalStatus = ApprovalStatus.Approved;
            doctor.RejectionReason = null;

            if (doctor.User != null)
                doctor.User.IsActive = true;

            await _context.SaveChangesAsync();

            if (doctor.User != null)
            {
                await _notificationService.SendAsync(
                    doctor.User.Id,
                    GhaithAI.GaithAI.Domain.Enums.NotificationType.ProfileApproved,
                    "Profile Approved",
                    "Congratulations! Your doctor profile has been approved. You can now start receiving bookings.",
                    doctor.Id);
            }
        }

        public async Task RejectDoctorAsync(Guid id, string reason)
        {
            var doctor = await _context.DoctorsProfiles
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            doctor.ApprovalStatus = ApprovalStatus.Rejected;
            doctor.RejectionReason = reason;

            await _context.SaveChangesAsync();

            if (doctor.User != null)
            {
                await _notificationService.SendAsync(
                    doctor.User.Id,
                    GhaithAI.GaithAI.Domain.Enums.NotificationType.ProfileRejected,
                    "Profile Rejected",
                    $"Your doctor profile has been rejected. Reason: {reason}",
                    doctor.Id);
            }
        }

        private DoctorProfileDTO MapToDTO(DoctorsProfile d) => new DoctorProfileDTO
        {
            Id = d.Id,
            FullName = d.FullName,
            DoctorType = d.DoctorType.ToString(),
            Specialization = d.Specialization,
            Bio = d.Bio,
            YearsOfExperience = d.YearsOfExperience,
            DocumentsPdfUrl = d.DocumentsPdfUrl,
            ApprovalStatus = d.ApprovalStatus.ToString(),
            RejectionReason = d.RejectionReason,
            AverageRating = d.AverageRating,
            Email = d.User?.Email ?? "",
            PhoneNumber = d.User?.PhoneNumber ?? "",
            ProfilePicture = d.User?.ProfilePicture
        };
    }
}