using GhaithAI.GaithAI.Application.DTOs.Admin;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorsProfile>>GetPendingDoctorsAsync()
        {
            return await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x =>
                    x.ApprovalStatus ==
                    ApprovalStatus.Pending)
                .ToListAsync();
        }

        public async Task<List<DoctorsProfile>>GetApprovedDoctorsAsync()
        {
            return await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x =>
                    x.ApprovalStatus ==
                    ApprovalStatus.Approved)
                .ToListAsync();
        }

        public async Task<List<DoctorsProfile>>GetRejectedDoctorsAsync()
        {
            return await _context.DoctorsProfiles
                .Include(x => x.User)
                .Where(x =>
                    x.ApprovalStatus ==
                    ApprovalStatus.Rejected)
                .ToListAsync();
        }

        public async Task<DoctorsProfile>GetDoctorDetailsAsync(Guid id)
        {
            return await _context.DoctorsProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);
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

            // ✅ تفعيل الـ user account
            if (doctor.User != null)
                doctor.User.IsActive = true;

            await _context.SaveChangesAsync();
        }

        public async Task RejectDoctorAsync(Guid id, string reason)
        {
            var doctor = await _context.DoctorsProfiles
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (doctor == null)
                throw new Exception("Doctor not found");

            doctor.ApprovalStatus = ApprovalStatus.Rejected;
            doctor.RejectionReason = reason;

            await _context.SaveChangesAsync();
        }
    }
}
