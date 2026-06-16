using GhaithAI.GaithAI.Application.DTOs.Booking;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;
using GhaithAI.GaithAI.Domain.Entities;

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


        public async Task<Guid> CreateUserBookingAsync(string userId, CreateUserBookingDto dto)
        {
            var doctor = await _unitOfWork.DoctorProfile.GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId &&
                                          d.ApprovalStatus == ApprovalStatus.Approved)
                ?? throw new KeyNotFoundException("Doctor not found or not approved.");

            var slotTaken = await _unitOfWork.Booking.GetAllQueryableNoTracking()
                .AnyAsync(b => b.DoctorId == dto.DoctorId &&
                               b.BookingDate.Date == dto.BookingDate.Date &&
                               b.SlotTime == dto.SlotTime &&
                               b.Status != BookingStatus.Cancelled);

            if (slotTaken)
                throw new InvalidOperationException("This slot is already booked.");

            var userHasConflict = await _unitOfWork.Booking.GetAllQueryableNoTracking()
                .AnyAsync(b => b.PatientId == userId &&
                               b.BookingDate.Date == dto.BookingDate.Date &&
                               b.SlotTime == dto.SlotTime &&
                               b.Status != BookingStatus.Cancelled);

            if (userHasConflict)
                throw new InvalidOperationException("You already have a booking at this time.");

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                PatientId = userId,
                ClinicPatientId = null,
                BookingDate = dto.BookingDate.Date,
                SlotTime = dto.SlotTime,
                SessionType = dto.SessionType,
                BookingSource = BookingSource.App,
                Status = BookingStatus.Confirmed,
                Notes = dto.BookingNotes ?? string.Empty,
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedBy = userId
            };

            await _unitOfWork.Booking.AddAsync(booking);
            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("An unexpected error occurred while saving the booking data.");

            return booking.Id;
        }

        // ─── Get User Bookings ────────────────────────────
        public async Task<IEnumerable<UserBookingResponseDto>> GetUserBookingsAsync(
            string userId,
            string? timeFilter,
            int pageIndex = 0,
            int pageSize = 10)
        {
            var query = _unitOfWork.Booking.GetAllQueryableNoTracking()
                .Include(b => b.Doctor)
                .Where(b => b.PatientId == userId);

            var today = DateTime.Today;

            if (!string.IsNullOrEmpty(timeFilter))
            {
                var filterLower = timeFilter.Trim().ToLower();
                query = filterLower switch
                {
                    "upcoming" => query.Where(b =>
                        b.BookingDate >= today &&
                        b.Status != BookingStatus.Cancelled),
                    "past" => query.Where(b => b.BookingDate < today),
                    "cancelled" => query.Where(b => b.Status == BookingStatus.Cancelled),
                    _ => query
                };
            }

            var items = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.SlotTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var now = DateTime.UtcNow;

            return items.Select(b =>
            {
                var bookingDateTime = b.BookingDate + b.SlotTime;
                var hoursUntil = (bookingDateTime - now).TotalHours;

                return new UserBookingResponseDto
                {
                    BookingId = b.Id,
                    DoctorId = b.DoctorId,
                    DoctorName = b.Doctor?.FullName ?? "N/A",
                    DoctorSpecialization = b.Doctor?.Specialization ?? "N/A",
                    BookingDate = b.BookingDate,
                    SlotTime = b.SlotTime,
                    DisplayTime = DateTime.Today.Add(b.SlotTime).ToString("hh:mm tt"),
                    SessionType = b.SessionType.ToString(),
                    Status = b.Status.ToString(),
                    Notes = b.Notes,
                    CanCancel = b.Status != BookingStatus.Cancelled && hoursUntil > 1
                };
            });
        }

        // ─── Cancel User Booking ──────────────────────────
        public async Task CancelUserBookingAsync(string userId, Guid bookingId)
        {
            var booking = await _unitOfWork.Booking.GetAllQueryableTracking()
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.PatientId == userId)
                ?? throw new KeyNotFoundException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            var bookingDateTime = booking.BookingDate + booking.SlotTime;
            var hoursUntil = (bookingDateTime - DateTime.UtcNow).TotalHours;

            if (hoursUntil < 1)
                throw new InvalidOperationException(
                    "Cannot cancel a booking less than 1 hour before the appointment.");

            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancelledBy = userId;

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
                throw new Exception("An unexpected error occurred while cancelling the booking.");
        }

        // ─── Get Available Slots ──────────────────────────
        public async Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateTime date)
        {
            var dayOfWeek = (DaysOfWeek)date.DayOfWeek;

            // شوف لو في custom schedule للتاريخ ده
            var customSchedule = await _unitOfWork.CustomSchedule
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId &&
                                          s.CustomDate.Date == date.Date);

            // لو يوم إجازة
            if (customSchedule?.IsOffDay == true)
                return Enumerable.Empty<AvailableSlotDto>();

            TimeSpan startTime;
            TimeSpan endTime;

            if (customSchedule != null)
            {
                startTime = customSchedule.StartTime;
                endTime = customSchedule.EndTime;
            }
            else
            {
                var defaultSchedule = await _unitOfWork.DoctorDefaultSchedule
                    .GetAllQueryableNoTracking()
                    .FirstOrDefaultAsync(s => s.DoctorId == doctorId &&
                                              s.DayOfWeek == dayOfWeek &&
                                              s.IsActive);

                if (defaultSchedule == null)
                    return Enumerable.Empty<AvailableSlotDto>();

                startTime = defaultSchedule.StartTime;
                endTime = defaultSchedule.EndTime;
            }

            // جيب الحجوزات الموجودة في اليوم ده
            var bookedSlots = await _unitOfWork.Booking
                .GetAllQueryableNoTracking()
                .Where(b => b.DoctorId == doctorId &&
                            b.BookingDate.Date == date.Date &&
                            b.Status != BookingStatus.Cancelled)
                .Select(b => b.SlotTime)
                .ToListAsync();

            // اعمل slots كل 30 دقيقة
            var slots = new List<AvailableSlotDto>();
            var current = startTime;

            while (current < endTime)
            {
                var isAvailable = !bookedSlots.Contains(current);

                // لو اليوم ده - مش هيبين الـ slots اللي فاتت
                if (date.Date == DateTime.UtcNow.Date)
                    isAvailable = isAvailable && current > DateTime.UtcNow.TimeOfDay;

                slots.Add(new AvailableSlotDto
                {
                    SlotTime = current,
                    DisplayTime = DateTime.Today.Add(current).ToString("hh:mm tt"),
                    IsAvailable = isAvailable
                });

                current = current.Add(TimeSpan.FromMinutes(30));
            }

            return slots;
        }
       
    }
}
