using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ClinicPatientService : IClinicPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClinicPatientService> _logger;

        public ClinicPatientService(IUnitOfWork unitOfWork, ILogger<ClinicPatientService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<DoctorClinicPatientListDto>> GetPatientsByClinicAsync(string userId , string? searchTerm,int pageNumber = 1,int pageSize = 10)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(userId);
            _logger.LogInformation("Fetching patients with Pagination & Search for Clinic ID: {ClinicId}", doctorId);

            var query =  _unitOfWork.ClinicPatient.GetAllQueryableNoTracking().Where(p => p.DoctorId == doctorId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var cleanedSearch = searchTerm.Trim().ToLower();
                query = query.Where(p =>p.PatientFullName.ToLower().Contains(cleanedSearch) || p.PatientPhone.Contains(cleanedSearch));
            }

            var patients = await query
                .OrderBy(p => p.PatientFullName) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return patients.Select(p => new DoctorClinicPatientListDto
            {
                Id = p.Id,
                DoctorId = p.DoctorId,
                PatientFullName = p.PatientFullName,
                PatientPhone = p.PatientPhone
            });
        }

        public async Task<DoctorClinicPatientDetailsDto> GetByIdAsync(Guid id, string userId)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid patient ID.");

            Guid doctorId = await GetDoctorIdByUserIdAsync(userId);

            var patient = await _unitOfWork.ClinicPatient.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null)
            {
                _logger.LogWarning("Patient with ID {PatientId} not found or doesn't belong to Doctor {DoctorId}.", id, doctorId);
                throw new KeyNotFoundException("Patient record does not exist or you do not have permission to view it.");
            }

            return new DoctorClinicPatientDetailsDto
            {
                Id = patient.Id,
                DoctorId = patient.DoctorId,
                PatientFullName = patient.PatientFullName,
                PatientPhone = patient.PatientPhone,
                Notes = patient.Notes
            };
        }

        public async Task<DoctorClinicPatientDetailsDto> CreateAsync(CreateClinicPatientDto dto , string userId)
        {
            Guid doctorId = await GetDoctorIdByUserIdAsync(userId);

            var trimmedName = dto.PatientFullName?.Trim();
            var trimmedPhone = dto.PatientPhone?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedName) || string.IsNullOrWhiteSpace(trimmedPhone))
                throw new ArgumentException("Patient name and phone are required.");

            var isDuplicate = await _unitOfWork.ClinicPatient.GetAllQueryableNoTracking()
                .AnyAsync(p => p.DoctorId == doctorId && p.PatientPhone == trimmedPhone);

            if (isDuplicate)
            {
                _logger.LogWarning("Validation failed: Phone {Phone} already exists in doctor {DoctorId}", trimmedPhone, doctorId);
                throw new InvalidOperationException("A patient with this phone number already exists in this clinic.");
            }

            var patient = new ClinicPatient
            {
                Id = Guid.NewGuid(),
                DoctorId =doctorId,
                PatientFullName = trimmedName,
                PatientPhone = trimmedPhone,
                Notes = dto.Notes?.Trim() ?? string.Empty
            };

            await _unitOfWork.ClinicPatient.AddAsync(patient);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Successfully created patient {PatientId} for doctor {DoctorId}", patient.Id, patient.DoctorId);

            return new DoctorClinicPatientDetailsDto
            {
                Id = patient.Id,
                DoctorId = patient.DoctorId,
                PatientFullName = patient.PatientFullName,
                PatientPhone = patient.PatientPhone,
                Notes = patient.Notes
            };
        }
        public async Task<bool> UpdateAsync(UpdateClinicPatientDto dto , string userId)
        {
            if (dto.Id == Guid.Empty) throw new ArgumentException("Invalid patient ID.");
            var doctorId = await GetDoctorIdByUserIdAsync(userId);
            var patient = await _unitOfWork.ClinicPatient.GetAllQueryableTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.Id && p.DoctorId == doctorId);

            if (patient == null)
                throw new KeyNotFoundException("Patient record does not exist or you do not have permission to update it.");

            var trimmedPhone = dto.PatientPhone?.Trim();

            var isDuplicate = await _unitOfWork.ClinicPatient.GetAllQueryableNoTracking()
                .AnyAsync(p => p.DoctorId == doctorId && p.PatientPhone == trimmedPhone && p.Id != dto.Id);

            if (isDuplicate)
                throw new InvalidOperationException("Another patient with this phone number already exists in your records.");

            patient.PatientFullName = dto.PatientFullName?.Trim();
            patient.PatientPhone = trimmedPhone;
            patient.Notes = dto.Notes?.Trim() ?? string.Empty;

            _unitOfWork.ClinicPatient.Update(patient);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<string> DeleteAsync(Guid id, string userId)
        {
            if (id == Guid.Empty) throw new ArgumentException("Invalid patient ID.");
            var doctorId = await GetDoctorIdByUserIdAsync(userId);
            var patient = await _unitOfWork.ClinicPatient.GetAllQueryableTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId);

            if (patient == null)
                throw new KeyNotFoundException("Patient record does not exist or you do not have permission to delete it."); // 🔥 ظبطنا الكلمة هنا لـ delete

            await _unitOfWork.ClinicPatient.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Hard deleted patient with ID: {PatientId}", id);
            return "Patient deleted successfully from the system.";
        }

        private async Task<Guid> GetDoctorIdByUserIdAsync(string userId)
        {
            var doctorProfile = await _unitOfWork.DoctorProfile.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
            {
                _logger.LogWarning("Operation failed: No doctor profile found for User ID {UserId}", userId);
                throw new KeyNotFoundException("Doctor profile not found in the system. Please ensure your profile is complete.");
            }

            return doctorProfile.Id;
        }
    }
}
