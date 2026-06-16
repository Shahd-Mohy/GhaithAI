using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IDoctorClinicProfileService
    {
        Task<DoctorClinicProfileDto> GetMyProfileAsync(string userId);

        Task<DoctorClinicProfileDto> UpdateMyProfileAsync(string userId, UpdateDoctorClinicProfileDto dto);


        Task<DoctorClinicProfileDto> SetPublicListingAsync(string userId, bool isPublicListed);

        Task<List<DefaultScheduleDto>> GetDefaultScheduleAsync(string userId);

        Task<List<DefaultScheduleDto>> UpsertDefaultScheduleAsync(string userId, List<UpsertScheduleDto> slots);

        Task<List<CustomScheduleDto>> GetCustomSchedulesAsync(string userId, DateTime? from, DateTime? to);

        Task<CustomScheduleDto> AddCustomScheduleAsync(string userId, UpsertCustomScheduleDto dto);

        Task<CustomScheduleDto> UpdateCustomScheduleAsync(string userId, Guid id, UpsertCustomScheduleDto dto);

        Task DeleteCustomScheduleAsync(string userId, Guid id);

        Task<PagedResult<PublicDoctorCardDto>> GetPublicDoctorsAsync(
            string? searchTerm,
            string? specialty,
            string? language,
            string? sessionType,   
            string? city,
            int pageNumber = 1,
            int pageSize = 10);

        Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(Guid doctorId);


        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime from, DateTime to);
    }
}
