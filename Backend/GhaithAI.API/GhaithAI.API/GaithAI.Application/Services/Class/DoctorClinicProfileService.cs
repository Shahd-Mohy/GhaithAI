using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class DoctorClinicProfileService : IDoctorClinicProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DoctorClinicProfileService> _logger;

        public DoctorClinicProfileService(
            IUnitOfWork unitOfWork,
            ILogger<DoctorClinicProfileService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // DOCTOR SIDE — GET

        public async Task<DoctorClinicProfileDto> GetMyProfileAsync(string userId)
        {
            _logger.LogInformation("Fetching clinic profile for user {UserId}", userId);

            var profile = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            return MapToProfileDto(profile);
        }

        // DOCTOR SIDE — UPDATE  

        public async Task<DoctorClinicProfileDto> UpdateMyProfileAsync(string userId, UpdateDoctorClinicProfileDto dto)
        {
            _logger.LogInformation("Updating clinic profile for user {UserId}", userId);

            // Load tracked entities so EF can detect changes
            var profile = await _unitOfWork.DoctorProfile
                .GetAllQueryableTracking()
                .Include(d => d.Clinic)
                .Include(d => d.ServiceSetting)
                .Include(d => d.DefaultSchedules)
                .Include(d => d.DoctorSpecialties)
                .Include(d => d.DoctorLanguages)
                .FirstOrDefaultAsync(d => d.UserId == userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            //1. DoctorsProfile
            profile.Specialization = dto.ProfessionalTitle?.Trim() ?? profile.Specialization;
            profile.Bio = dto.Bio?.Trim() ?? profile.Bio;
            profile.YearsOfExperience = dto.YearsOfExperience;

            //  2. Clinic
            if (profile.Clinic == null)
            {
                profile.Clinic = new Clinic
                {
                    Id = Guid.NewGuid(),
                    DoctorId = profile.Id
                };
            }

            profile.Clinic.ClinicName = dto.ClinicName?.Trim() ?? profile.Clinic.ClinicName;
            profile.Clinic.ClinicAddress = dto.ClinicAddress?.Trim() ?? profile.Clinic.ClinicAddress;
            profile.Clinic.City = dto.City?.Trim() ?? profile.Clinic.City;
            profile.Clinic.CountryCode = dto.CountryCode?.Trim() ?? profile.Clinic.CountryCode;
            profile.Clinic.Phone = dto.Phone?.Trim() ?? profile.Clinic.Phone;
            profile.Clinic.ContactEmail = dto.ContactEmail?.Trim() ?? profile.Clinic.ContactEmail;
            profile.Clinic.IsPublicListed = dto.IsPublicListed;


            profile.FullName = dto.DisplayName?.Trim() ?? profile.FullName;

            if (profile.ServiceSetting == null)
            {
                profile.ServiceSetting = new DoctorServiceSetting
                {
                    Id = Guid.NewGuid(),
                    DoctorId = profile.Id
                };
            }

            profile.ServiceSetting.FeePerSession = dto.FeePerSession;
            profile.ServiceSetting.SessionDurationMinutes = dto.SessionDurationMinutes;
            profile.ServiceSetting.AvailableSessionType = dto.AvailableSessionType;

            UpdateSpecialties(profile, dto.SpecialtyIds);

            UpdateLanguages(profile, dto.LanguageIds);

            UpdateDefaultSchedule(profile, dto.WeeklySchedule);

            profile.UpdatedAt = DateTime.UtcNow;
            profile.UpdatedBy = userId;

            _unitOfWork.DoctorProfile.Update(profile);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Clinic profile updated for user {UserId}", userId);

            var updated = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)!;
            return MapToProfileDto(updated!);
        }

        // PATIENT SIDE —  listing

        public async Task<IEnumerable<PublicDoctorCardDto>> GetPublicDoctorsAsync(
            string? searchTerm,
            string? specialty,
            string? language,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _unitOfWork.DoctorProfile.GetPublicDoctorsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(d =>
                    d.FullName.ToLower().Contains(term) ||
                    d.Specialization.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                query = query.Where(d =>
                    d.DoctorSpecialties.Any(ds =>
                        ds.BaseSpecialty.SpecialtyName.ToLower() == specialty.Trim().ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(d =>
                    d.DoctorLanguages.Any(dl =>
                        dl.BaseLanguage.LanguageName.ToLower() == language.Trim().ToLower()));
            }

            var doctors = await query
                .OrderByDescending(d => d.AverageRating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return doctors.Select(MapToCardDto);
        }

        // PATIENT SIDE — full profile

        public async Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(Guid doctorId)
        {
            var profile = await _unitOfWork.DoctorProfile.GetFullProfileByDoctorIdAsync(doctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            if (profile.Clinic == null || !profile.Clinic.IsPublicListed)
                throw new UnauthorizedAccessException("This doctor's profile is not publicly listed.");

            return MapToFullPublicDto(profile);
        }

        // mapping

        private static DoctorClinicProfileDto MapToProfileDto(DoctorsProfile d) => new()
        {
            ClinicName = d.Clinic?.ClinicName ?? string.Empty,
            DisplayName = d.FullName,
            ProfessionalTitle = d.Specialization,
            YearsOfExperience = d.YearsOfExperience,
            Bio = d.Bio,
            IsPublicListed = d.Clinic?.IsPublicListed ?? false,
            ClinicAddress = d.Clinic?.ClinicAddress ?? string.Empty,
            City = d.Clinic?.City ?? string.Empty,
            CountryCode = d.Clinic?.CountryCode ?? string.Empty,
            Phone = d.Clinic?.Phone ?? string.Empty,
            ContactEmail = d.Clinic?.ContactEmail ?? string.Empty,
            FeePerSession = d.ServiceSetting?.FeePerSession ?? 0,
            SessionDurationMinutes = d.ServiceSetting?.SessionDurationMinutes ?? 0,
            AvailableSessionType = d.ServiceSetting?.AvailableSessionType ?? default,
            Specialties = d.DoctorSpecialties
                                     .Select(ds => new SpecialtyItemDto
                                     {
                                         Id = ds.SpecialtyId,
                                         Name = ds.BaseSpecialty?.SpecialtyName ?? string.Empty
                                     }).ToList(),
            Languages = d.DoctorLanguages
                                     .Select(dl => new LanguageItemDto
                                     {
                                         Id = dl.BaseLanguageId,
                                         Name = dl.BaseLanguage?.LanguageName ?? string.Empty
                                     }).ToList(),
            WeeklySchedule = d.DefaultSchedules
                                     .OrderBy(s => s.DayOfWeek)
                                     .Select(s => new DefaultScheduleDto
                                     {
                                         Id = s.Id,
                                         DayOfWeek = s.DayOfWeek,
                                         StartTime = s.StartTime,
                                         EndTime = s.EndTime,
                                         IsActive = s.IsActive
                                     }).ToList()
        };

        private static PublicDoctorCardDto MapToCardDto(DoctorsProfile d) => new()
        {
            DoctorId = d.Id,
            DisplayName = d.FullName,
            ProfessionalTitle = d.Specialization,
            AverageRating = d.AverageRating,
            ReviewCount = d.Reviews?.Count ?? 0,
            YearsOfExperience = d.YearsOfExperience,
            City = d.Clinic?.City ?? string.Empty,
            CountryCode = d.Clinic?.CountryCode ?? string.Empty,
            Bio = d.Bio,
            FeePerSession = d.ServiceSetting?.FeePerSession ?? 0,
            AvailableSessionType = d.ServiceSetting?.AvailableSessionType ?? default,
            Specialties = d.DoctorSpecialties
                                     .Select(ds => ds.BaseSpecialty?.SpecialtyName ?? string.Empty)
                                     .ToList(),
            Languages = d.DoctorLanguages
                                     .Select(dl => dl.BaseLanguage?.LanguageName ?? string.Empty)
                                     .ToList(),
            NextAvailableSlot = ResolveNextAvailableSlot(d.DefaultSchedules)
        };

        private static PublicDoctorProfileDto MapToFullPublicDto(DoctorsProfile d) => new()
        {
            DoctorId = d.Id,
            DisplayName = d.FullName,
            ProfessionalTitle = d.Specialization,
            AverageRating = d.AverageRating,
            ReviewCount = d.Reviews?.Count ?? 0,
            YearsOfExperience = d.YearsOfExperience,
            City = d.Clinic?.City ?? string.Empty,
            CountryCode = d.Clinic?.CountryCode ?? string.Empty,
            Bio = d.Bio,
            FeePerSession = d.ServiceSetting?.FeePerSession ?? 0,
            AvailableSessionType = d.ServiceSetting?.AvailableSessionType ?? default,
            Specialties = d.DoctorSpecialties
                                     .Select(ds => ds.BaseSpecialty?.SpecialtyName ?? string.Empty)
                                     .ToList(),
            Languages = d.DoctorLanguages
                                     .Select(dl => dl.BaseLanguage?.LanguageName ?? string.Empty)
                                     .ToList(),
            NextAvailableSlot = ResolveNextAvailableSlot(d.DefaultSchedules),
            WeeklySchedule = d.DefaultSchedules
                                     .OrderBy(s => s.DayOfWeek)
                                     .Select(s => new DefaultScheduleDto
                                     {
                                         Id = s.Id,
                                         DayOfWeek = s.DayOfWeek,
                                         StartTime = s.StartTime,
                                         EndTime = s.EndTime,
                                         IsActive = s.IsActive
                                     }).ToList()
        };

        private static string ResolveNextAvailableSlot(ICollection<DoctorDefaultSchedule>? schedules)
        {
            if (schedules == null || !schedules.Any(s => s.IsActive))
                return "Not available";

            var today = DateTime.UtcNow;

            for (int i = 0; i < 7; i++)
            {
                var candidate = today.AddDays(i);
                var dayEnum = (GhaithAI.GaithAI.Domain.Enums.DaysOfWeek)((int)candidate.DayOfWeek);
                var slot = schedules.FirstOrDefault(s => s.DayOfWeek == dayEnum && s.IsActive);

                if (slot != null)
                {
                    var label = i == 0 ? "Today" : i == 1 ? "Tomorrow" : candidate.ToString("ddd, MMM d");
                    return $"{label}, {slot.StartTime:hh\\:mm tt}";
                }
            }

            return "Check schedule";
        }


        private static void UpdateSpecialties(DoctorsProfile profile, List<Guid> newIds)
        {
            var toRemove = profile.DoctorSpecialties
                .Where(ds => !newIds.Contains(ds.SpecialtyId))
                .ToList();
            foreach (var r in toRemove)
                profile.DoctorSpecialties.Remove(r);

            var existingIds = profile.DoctorSpecialties.Select(ds => ds.SpecialtyId).ToHashSet();
            foreach (var id in newIds.Where(id => !existingIds.Contains(id)))
            {
                profile.DoctorSpecialties.Add(new DoctorSpecialty
                {
                    Id = Guid.NewGuid(),
                    DoctorId = profile.Id,
                    SpecialtyId = id
                });
            }
        }

        private static void UpdateLanguages(DoctorsProfile profile, List<Guid> newIds)
        {
            var toRemove = profile.DoctorLanguages
                .Where(dl => !newIds.Contains(dl.BaseLanguageId))
                .ToList();
            foreach (var r in toRemove)
                profile.DoctorLanguages.Remove(r);

            var existingIds = profile.DoctorLanguages.Select(dl => dl.BaseLanguageId).ToHashSet();
            foreach (var id in newIds.Where(id => !existingIds.Contains(id)))
            {
                profile.DoctorLanguages.Add(new DoctorLanguage
                {
                    Id = Guid.NewGuid(),
                    DoctorId = profile.Id,
                    BaseLanguageId = id
                });
            }
        }

        private static void UpdateDefaultSchedule(DoctorsProfile profile, List<UpsertScheduleDto> desired)
        {

            profile.DefaultSchedules.Clear();

            foreach (var s in desired)
            {
                profile.DefaultSchedules.Add(new DoctorDefaultSchedule
                {
                    Id = Guid.NewGuid(),
                    DoctorId = profile.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsActive = s.IsActive
                });
            }
        }
    }
}
