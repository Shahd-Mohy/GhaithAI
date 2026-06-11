using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IDoctorClinicProfileService
    {
        // Doctor side 

        Task<DoctorClinicProfileDto> GetMyProfileAsync(string userId);

        Task<DoctorClinicProfileDto> UpdateMyProfileAsync(string userId, UpdateDoctorClinicProfileDto dto);

        //  Patient side

        Task<IEnumerable<PublicDoctorCardDto>> GetPublicDoctorsAsync(
            string? searchTerm,
            string? specialty,
            string? language,
            int pageNumber = 1,
            int pageSize = 10);

        Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(Guid doctorId);
    }
}
