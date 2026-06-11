using GhaithAI.GaithAI.Application.DTOs.Admin;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IAdminService
    {
        Task<List<DoctorsProfile>> GetPendingDoctorsAsync();

        Task<List<DoctorsProfile>> GetApprovedDoctorsAsync();

        Task<List<DoctorsProfile>> GetRejectedDoctorsAsync();

        Task<DoctorsProfile> GetDoctorDetailsAsync(Guid id);

        Task ApproveDoctorAsync(Guid id);

        Task RejectDoctorAsync(
            Guid id,
            string reason);
    }
}
