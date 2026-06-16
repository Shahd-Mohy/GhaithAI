using GhaithAI.GaithAI.Application.DTOs.Admin;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IAdminService
    {
        Task<List<DoctorProfileDTO>> GetPendingDoctorsAsync();
        Task<List<DoctorProfileDTO>> GetApprovedDoctorsAsync();
        Task<List<DoctorProfileDTO>> GetRejectedDoctorsAsync();
        Task<DoctorProfileDTO> GetDoctorDetailsAsync(Guid id);
        Task ApproveDoctorAsync(Guid id);
        Task RejectDoctorAsync(Guid id, string reason);
    }
}