using GhaithAI.GaithAI.Application.DTOs.Booking;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; 

        public BookingService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                query = timeFilter.ToLower() switch
                {
                    "upcoming" => query.Where(b => b.BookingDate >= today && b.Status != BookingStatus.Cancelled),
                    "past" => query.Where(b => b.BookingDate < today),
                    _ => query
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