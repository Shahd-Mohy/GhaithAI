using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IClinicPatientService
    {
        Task<IEnumerable<DoctorClinicPatientListDto>> GetPatientsByClinicAsync(string userId, string? searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<DoctorClinicPatientDetailsDto> GetByIdAsync(Guid id , string userId);
        Task<DoctorClinicPatientDetailsDto> CreateAsync(CreateClinicPatientDto dto ,string userId);
        Task<bool> UpdateAsync(UpdateClinicPatientDto dto , string userId);
        Task<string> DeleteAsync(Guid id , string userId);
    }
}
