using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IClinicPatientService
    {
        Task<IEnumerable<DoctorClinicPatientListDto>> GetPatientsByClinicAsync(Guid clinicId, string? searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<DoctorClinicPatientDetailsDto> GetByIdAsync(Guid id);
        Task<DoctorClinicPatientDetailsDto> CreateAsync(CreateClinicPatientDto dto);
        Task<bool> UpdateAsync(UpdateClinicPatientDto dto);
        Task<string> DeleteAsync(Guid id);
    }
}
