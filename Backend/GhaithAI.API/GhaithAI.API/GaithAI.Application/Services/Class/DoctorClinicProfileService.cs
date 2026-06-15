using GhaithAI.API.Repositories.UnitWork;
using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Application.Services
{
    public class DoctorClinicProfileService : IDoctorClinicProfileService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorClinicProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorClinicProfileDto> GetMyProfileAsync(string userId)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            return MapToProfileDto(doctor);
        }

        public async Task<DoctorClinicProfileDto> UpdateMyProfileAsync(string userId, UpdateDoctorClinicProfileDto dto)
        {
            var doctor = await _unitOfWork.DoctorProfile
                .GetProfileForUpdateQueryable()
                .FirstOrDefaultAsync(d => d.UserId == userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            doctor.FullName = dto.DisplayName;
            doctor.Specialization = dto.ProfessionalTitle;
            doctor.Bio = dto.Bio;
            doctor.YearsOfExperience = dto.YearsOfExperience;

            if (doctor.Clinic == null)
            {
                doctor.Clinic = new Clinic { DoctorId = doctor.Id };
            }
            doctor.Clinic.ClinicName = dto.ClinicName;
            doctor.Clinic.Phone = dto.Phone;
            doctor.Clinic.ContactEmail = dto.ContactEmail;
            doctor.Clinic.IsPublicListed = dto.IsPublicListed;

            if (dto.AvailableSessionType != SessionType.Online)
            {
                doctor.Clinic.ClinicAddress = dto.ClinicAddress;
                doctor.Clinic.City = dto.City;
                doctor.Clinic.CountryCode = dto.CountryCode;
            }

            if (doctor.ServiceSetting == null)
            {
                doctor.ServiceSetting = new DoctorServiceSetting { DoctorId = doctor.Id };
            }
            doctor.ServiceSetting.FeePerSession = dto.FeePerSession;
            doctor.ServiceSetting.SessionDurationMinutes = dto.SessionDurationMinutes;
            doctor.ServiceSetting.AvailableSessionType = dto.AvailableSessionType;

            var existingSpecialtyNames = doctor.DoctorSpecialties
                .Select(s => s.BaseSpecialty.SpecialtyName.ToLower())
                .ToHashSet();

            var requestedNames = dto.Specialties.Select(s => s.ToLower()).ToHashSet();

            var toRemoveSpecialties = doctor.DoctorSpecialties
                .Where(s => !requestedNames.Contains(s.BaseSpecialty.SpecialtyName.ToLower()))
                .ToList();

            foreach (var s in toRemoveSpecialties)
                _unitOfWork.DoctorSpecialty.Delete(s);

            foreach (var specialtyName in dto.Specialties)
            {
                if (!existingSpecialtyNames.Contains(specialtyName.ToLower()))
                {
                    var baseSpecialty = await _unitOfWork.BaseSpecialty
                        .GetAllQueryableNoTracking()
                        .FirstOrDefaultAsync(b => b.SpecialtyName.ToLower() == specialtyName.ToLower());

                    if (baseSpecialty != null)
                    {
                        await _unitOfWork.DoctorSpecialty.AddAsync(new DoctorSpecialty
                        {
                            DoctorId = doctor.Id,
                            SpecialtyId = baseSpecialty.Id
                        });
                    }
                }
            }

            var existingLanguageNames = doctor.DoctorLanguages
                .Select(l => l.BaseLanguage.LanguageName.ToLower())
                .ToHashSet();

            var requestedLanguages = dto.Languages.Select(l => l.ToLower()).ToHashSet();

            var toRemoveLanguages = doctor.DoctorLanguages
                .Where(l => !requestedLanguages.Contains(l.BaseLanguage.LanguageName.ToLower()))
                .ToList();

            foreach (var l in toRemoveLanguages)
                _unitOfWork.DoctorLanguage.Delete(l);

            foreach (var languageName in dto.Languages)
            {
                if (!existingLanguageNames.Contains(languageName.ToLower()))
                {
                    var baseLang = await _unitOfWork.BaseLanguage
                        .GetAllQueryableNoTracking()
                        .FirstOrDefaultAsync(b => b.LanguageName.ToLower() == languageName.ToLower());

                    if (baseLang != null)
                    {
                        await _unitOfWork.DoctorLanguage.AddAsync(new DoctorLanguage
                        {
                            DoctorId = doctor.Id,
                            BaseLanguageId = baseLang.Id
                        });
                    }
                }
            }

            var existingSchedules = doctor.DefaultSchedules.ToList();
            foreach (var existing in existingSchedules)
                _unitOfWork.DoctorDefaultSchedule.Delete(existing);

            foreach (var slot in dto.WeeklySchedule)
            {
                await _unitOfWork.DoctorDefaultSchedule.AddAsync(new DoctorDefaultSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = slot.DayOfWeek,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsActive = slot.IsActive
                });
            }

            _unitOfWork.DoctorProfile.Update(doctor);
            await _unitOfWork.CompleteAsync();

            return await GetMyProfileAsync(userId);
        }

        public async Task<DoctorClinicProfileDto> SetPublicListingAsync(string userId, bool isPublicListed)
        {
            var doctor = await _unitOfWork.DoctorProfile
                .GetProfileForUpdateQueryable()
                .FirstOrDefaultAsync(d => d.UserId == userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            if (doctor.Clinic == null)
                throw new InvalidOperationException("Clinic profile not set up yet.");

            doctor.Clinic.IsPublicListed = isPublicListed;
            await _unitOfWork.CompleteAsync();

            return await GetMyProfileAsync(userId);
        }

        public async Task<List<DefaultScheduleDto>> GetDefaultScheduleAsync(string userId)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            return doctor.DefaultSchedules
                .OrderBy(s => s.DayOfWeek)
                .Select(s => new DefaultScheduleDto
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsActive = s.IsActive
                })
                .ToList();
        }

        public async Task<List<DefaultScheduleDto>> UpsertDefaultScheduleAsync(string userId, List<UpsertScheduleDto> slots)
        {
            var doctor = await _unitOfWork.DoctorProfile
                .GetProfileForUpdateQueryable()
                .FirstOrDefaultAsync(d => d.UserId == userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var existing = doctor.DefaultSchedules.ToList();
            foreach (var s in existing)
                _unitOfWork.DoctorDefaultSchedule.Delete(s);

            foreach (var slot in slots)
            {
                await _unitOfWork.DoctorDefaultSchedule.AddAsync(new DoctorDefaultSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = slot.DayOfWeek,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    IsActive = slot.IsActive
                });
            }

            await _unitOfWork.CompleteAsync();

            return await GetDefaultScheduleAsync(userId);
        }

        public async Task<List<CustomScheduleDto>> GetCustomSchedulesAsync(string userId, DateTime? from, DateTime? to)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var schedules = await _unitOfWork.DoctorProfile
                .GetCustomSchedulesAsync(doctor.Id, from, to);

            return schedules.Select(MapToCustomScheduleDto).ToList();
        }

        public async Task<CustomScheduleDto> AddCustomScheduleAsync(string userId, UpsertCustomScheduleDto dto)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var entity = new DoctorCustomSchedule
            {
                DoctorId = doctor.Id,
                CustomDate = dto.CustomDate.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsOffDay = dto.IsOffDay
            };

            await _unitOfWork.CustomSchedule.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return MapToCustomScheduleDto(entity);
        }

        public async Task<CustomScheduleDto> UpdateCustomScheduleAsync(string userId, Guid id, UpsertCustomScheduleDto dto)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var entity = await _unitOfWork.CustomSchedule.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Custom schedule not found.");

            if (entity.DoctorId != doctor.Id)
                throw new UnauthorizedAccessException("Access denied.");

            entity.CustomDate = dto.CustomDate.Date;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
            entity.IsOffDay = dto.IsOffDay;

            _unitOfWork.CustomSchedule.Update(entity);
            await _unitOfWork.CompleteAsync();

            return MapToCustomScheduleDto(entity);
        }

        public async Task DeleteCustomScheduleAsync(string userId, Guid id)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByUserIdAsync(userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var entity = await _unitOfWork.CustomSchedule.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Custom schedule not found.");

            if (entity.DoctorId != doctor.Id)
                throw new UnauthorizedAccessException("Access denied.");

            _unitOfWork.CustomSchedule.Delete(entity);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResult<PublicDoctorCardDto>> GetPublicDoctorsAsync(
            string? searchTerm,
            string? specialty,
            string? language,
            string? sessionType,
            string? city,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _unitOfWork.DoctorProfile.GetPublicDoctorsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(d =>
                    d.FullName.ToLower().Contains(term) ||
                    d.Specialization.ToLower().Contains(term) ||
                    d.Bio.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(specialty))
            {
                var sp = specialty.ToLower();
                query = query.Where(d =>
                    d.DoctorSpecialties.Any(s => s.BaseSpecialty.SpecialtyName.ToLower() == sp));
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                var lang = language.ToLower();
                query = query.Where(d =>
                    d.DoctorLanguages.Any(l => l.BaseLanguage.LanguageName.ToLower() == lang));
            }

            if (!string.IsNullOrWhiteSpace(sessionType) &&
                Enum.TryParse<SessionType>(sessionType, true, out var sessionTypeEnum))
            {
                query = query.Where(d =>
                    d.ServiceSetting != null &&
                    (d.ServiceSetting.AvailableSessionType == sessionTypeEnum ||
                     d.ServiceSetting.AvailableSessionType == SessionType.both));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = city.ToLower();
                query = query.Where(d => d.Clinic != null && d.Clinic.City.ToLower() == c);
            }

            var totalCount = await query.CountAsync();

            var doctors = await query
                .OrderByDescending(d => d.AverageRating)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = doctors.Select(d => new PublicDoctorCardDto
            {
                DoctorId = d.Id,
                DisplayName = d.FullName,
                ProfessionalTitle = d.Specialization,
                AverageRating = d.AverageRating,
                ReviewCount = d.Reviews?.Count ?? 0,
                YearsOfExperience = d.YearsOfExperience,
                City = d.Clinic?.City,
                CountryCode = d.Clinic?.CountryCode,
                Bio = d.Bio,
                FeePerSession = d.ServiceSetting?.FeePerSession ?? 0,
                AvailableSessionType = d.ServiceSetting?.AvailableSessionType ?? SessionType.both,
                Specialties = d.DoctorSpecialties?.Select(s => s.BaseSpecialty.SpecialtyName).ToList() ?? new(),
                Languages = d.DoctorLanguages?.Select(l => l.BaseLanguage.LanguageName).ToList() ?? new(),
                NextAvailableSlot = GetNextAvailableSlot(d)
            }).ToList();

            return new PagedResult<PublicDoctorCardDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(Guid doctorId)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByDoctorIdAsync(doctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            if (doctor.Clinic == null || !doctor.Clinic.IsPublicListed)
                throw new KeyNotFoundException("Doctor not found.");

            return new PublicDoctorProfileDto
            {
                DoctorId = doctor.Id,
                DisplayName = doctor.FullName,
                ProfessionalTitle = doctor.Specialization,
                AverageRating = doctor.AverageRating,
                ReviewCount = doctor.Reviews?.Count ?? 0,
                YearsOfExperience = doctor.YearsOfExperience,
                City = doctor.Clinic?.City,
                CountryCode = doctor.Clinic?.CountryCode,
                Bio = doctor.Bio,
                FeePerSession = doctor.ServiceSetting?.FeePerSession ?? 0,
                AvailableSessionType = doctor.ServiceSetting?.AvailableSessionType ?? SessionType.both,
                Specialties = doctor.DoctorSpecialties?.Select(s => s.BaseSpecialty.SpecialtyName).ToList() ?? new(),
                Languages = doctor.DoctorLanguages?.Select(l => l.BaseLanguage.LanguageName).ToList() ?? new(),
                NextAvailableSlot = GetNextAvailableSlot(doctor),
                WeeklySchedule = doctor.DefaultSchedules?
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DayOfWeek)
                    .Select(s => new DefaultScheduleDto
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        IsActive = s.IsActive
                    }).ToList() ?? new()
            };
        }

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(Guid doctorId, DateTime from, DateTime to)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetFullProfileByDoctorIdAsync(doctorId)
                ?? throw new KeyNotFoundException("Doctor not found.");

            if (doctor.Clinic == null || !doctor.Clinic.IsPublicListed)
                throw new KeyNotFoundException("Doctor not found.");

            var sessionDuration = doctor.ServiceSetting?.SessionDurationMinutes ?? 50;

            var existingBookings = await _unitOfWork.Booking
                .GetAllQueryableNoTracking()
                .Where(b => b.DoctorId == doctorId &&
                            b.BookingDate.Date >= from.Date &&
                            b.BookingDate.Date <= to.Date &&
                            b.Status == BookingStatus.Confirmed)
                .ToListAsync();

            var customSchedules = await _unitOfWork.DoctorProfile
                .GetCustomSchedulesAsync(doctorId, from, to);

            var slots = new List<AvailableSlotDto>();
            var current = from.Date;

            while (current <= to.Date)
            {
                var dayOfWeek = (DaysOfWeek)current.DayOfWeek;

                var customDay = customSchedules
                    .Where(cs => cs.CustomDate.Date == current)
                    .ToList();

                if (customDay.Any(cs => cs.IsOffDay))
                {
                    current = current.AddDays(1);
                    continue;
                }

                List<(TimeSpan start, TimeSpan end)> windows = new();

                if (customDay.Any(cs => !cs.IsOffDay))
                {
                    windows = customDay
                        .Where(cs => !cs.IsOffDay)
                        .Select(cs => (cs.StartTime, cs.EndTime))
                        .ToList();
                }
                else
                {
                    var defaultSchedule = doctor.DefaultSchedules
                        .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);

                    if (defaultSchedule != null)
                        windows.Add((defaultSchedule.StartTime, defaultSchedule.EndTime));
                }

                foreach (var (start, end) in windows)
                {
                    var slotStart = start;
                    while (slotStart.Add(TimeSpan.FromMinutes(sessionDuration)) <= end)
                    {
                        var slotEnd = slotStart.Add(TimeSpan.FromMinutes(sessionDuration));
                        var isBooked = existingBookings.Any(b =>
                            b.BookingDate.Date == current && b.SlotTime == slotStart);

                        if (!isBooked)
                        {
                            slots.Add(new AvailableSlotDto
                            {
                                Date = current,
                                StartTime = slotStart,
                                EndTime = slotEnd
                            });
                        }

                        slotStart = slotEnd;
                    }
                }

                current = current.AddDays(1);
            }

            return slots;
        }

        private static DoctorClinicProfileDto MapToProfileDto(DoctorsProfile doctor)
        {
            return new DoctorClinicProfileDto
            {
                DisplayName = doctor.FullName,
                ProfessionalTitle = doctor.Specialization,
                Bio = doctor.Bio,
                YearsOfExperience = doctor.YearsOfExperience,
                PracticeType = doctor.DoctorType.ToString(),
                ClinicName = doctor.Clinic?.ClinicName,
                ClinicAddress = doctor.Clinic?.ClinicAddress,
                City = doctor.Clinic?.City,
                CountryCode = doctor.Clinic?.CountryCode,
                Phone = doctor.Clinic?.Phone,
                ContactEmail = doctor.Clinic?.ContactEmail,
                IsPublicListed = doctor.Clinic?.IsPublicListed ?? false,
                FeePerSession = doctor.ServiceSetting?.FeePerSession ?? 0,
                SessionDurationMinutes = doctor.ServiceSetting?.SessionDurationMinutes ?? 0,
                AvailableSessionType = doctor.ServiceSetting?.AvailableSessionType ?? SessionType.both,
                Specialties = doctor.DoctorSpecialties?.Select(s => s.BaseSpecialty.SpecialtyName).ToList() ?? new(),
                Languages = doctor.DoctorLanguages?.Select(l => l.BaseLanguage.LanguageName).ToList() ?? new(),
                WeeklySchedule = doctor.DefaultSchedules?
                    .OrderBy(s => s.DayOfWeek)
                    .Select(s => new DefaultScheduleDto
                    {
                        Id = s.Id,
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        IsActive = s.IsActive
                    }).ToList() ?? new()
            };
        }

        private static CustomScheduleDto MapToCustomScheduleDto(DoctorCustomSchedule entity)
        {
            return new CustomScheduleDto
            {
                Id = entity.Id,
                CustomDate = entity.CustomDate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                IsOffDay = entity.IsOffDay
            };
        }

        private static string? GetNextAvailableSlot(DoctorsProfile doctor)
        {
            if (doctor.DefaultSchedules == null || !doctor.DefaultSchedules.Any())
                return null;

            var today = DateTime.UtcNow;
            var todayDow = (DaysOfWeek)today.DayOfWeek;

            for (int i = 0; i <= 7; i++)
            {
                var checkDay = (DaysOfWeek)(((int)todayDow + i) % 7);
                var schedule = doctor.DefaultSchedules
                    .FirstOrDefault(s => s.DayOfWeek == checkDay && s.IsActive);

                if (schedule != null)
                {
                    var date = today.Date.AddDays(i);
                    var label = i == 0 ? "Today" : i == 1 ? "Tomorrow" : date.ToString("ddd, MMM d");
                    return $"{label}, {schedule.StartTime:hh\\:mm} AM";
                }
            }

            return null;
        }
    }
}