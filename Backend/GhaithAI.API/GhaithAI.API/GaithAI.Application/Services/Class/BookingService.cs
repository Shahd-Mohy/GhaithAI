using GhaithAI.GaithAI.Application.DTOs.Booking;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IClinicPatientService _patientService; 

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper, IClinicPatientService patientService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _patientService = patientService;
        }

        public async Task<IEnumerable<DoctorBookingResponseDto>> GetDoctorBookingsPagedAsync(
     string userId,
     string? timeFilter,
     int pageIndex = 0,
     int pageSize = 10)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(userId);

            var query = _unitOfWork.Booking.GetAllQueryableNoTracking()
                .Include(b => b.Patient)
                .Include(b => b.ClinicPatient)
                .Where(b => b.DoctorId == doctorId);

            var today = DateTime.Today;

            if (!string.IsNullOrEmpty(timeFilter))
            {
                var filterLower = timeFilter.Trim().ToLower();

                query = filterLower switch
                {
                    "all" => query,
                    "today" => query.Where(b => b.BookingDate == today),
                    "tomorrow" => query.Where(b => b.BookingDate == today.AddDays(1)),
                    "upcoming" => query.Where(b => b.BookingDate >= today && b.Status != BookingStatus.Cancelled),
                    "past" => query.Where(b => b.BookingDate < today),
                    _ => DateTime.TryParse(timeFilter, out DateTime targetDate)
                            ? query.Where(b => b.BookingDate == targetDate.Date)
                            : query
                };
            }

            var items = await query
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.SlotTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<DoctorBookingResponseDto>>(items); 
        }
        public async Task<Guid> CreateClinicBookingAsync(string userId, CreateClinicBookingDto dto)
        {
            var doctorId = await GetDoctorIdByUserIdAsync(userId);

            bool isSlotBusy = await _unitOfWork.Booking.GetAllQueryableNoTracking()
                .AnyAsync(b => b.DoctorId == doctorId &&
                               b.BookingDate == dto.BookingDate.Date &&
                               b.SlotTime == dto.SlotTime &&
                               b.Status != BookingStatus.Cancelled);

            if (isSlotBusy)
            {
                throw new InvalidOperationException("This time slot is already booked. Please choose another time.");
            }

            Guid finalClinicPatientId;

            if (dto.ClinicPatientId.HasValue)
            {
                var patientExists = await _unitOfWork.ClinicPatient.GetAllQueryableNoTracking()
                    .AnyAsync(p => p.Id == dto.ClinicPatientId.Value && p.DoctorId == doctorId);

                if (!patientExists)
                {
                    throw new KeyNotFoundException("Patient profile not found or does not belong to this clinic.");
                }

                finalClinicPatientId = dto.ClinicPatientId.Value;
            }
            else
            {
                if (dto.NewPatientInfo == null)
                {
                    throw new ArgumentException("New patient information is required.");
                }

                var createdPatient = await _patientService.CreateAsync(dto.NewPatientInfo, userId);
                finalClinicPatientId = createdPatient.Id;
            }

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                ClinicPatientId = finalClinicPatientId,
                PatientId = null,
                BookingDate = dto.BookingDate.Date,
                SlotTime = dto.SlotTime,
                SessionType = dto.SessionType,
                BookingSource = BookingSource.Manual,
                Status = BookingStatus.Confirmed,
                Notes = dto.BookingNotes ?? string.Empty,
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedBy = userId
            };

            await _unitOfWork.Booking.AddAsync(booking);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
            {
                throw new Exception("An unexpected error occurred while saving the booking data.");
            }

            return booking.Id;
        }

        private async Task<Guid> GetDoctorIdByUserIdAsync(string userId)
        {
            var doctorProfile = await _unitOfWork.DoctorProfile.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
            {
                //_logger.LogWarning("Operation failed: No doctor profile found for User ID {UserId}", userId);
                throw new KeyNotFoundException("Doctor profile not found in the system. Please ensure your profile is complete.");
            }

            return doctorProfile.Id;
        }
    }
}