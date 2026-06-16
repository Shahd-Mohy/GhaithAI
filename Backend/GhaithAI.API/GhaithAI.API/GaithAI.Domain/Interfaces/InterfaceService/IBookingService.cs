using GhaithAI.GaithAI.Application.DTOs.Booking;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IBookingService
    {
        Task<IEnumerable<DoctorBookingResponseDto>> GetDoctorBookingsPagedAsync(
              string userId,
              string? timeFilter,
              int pageIndex = 0,
              int pageSize = 10);

        Task<Guid> CreateClinicBookingAsync(string userId, CreateClinicBookingDto dto);

        Task<Guid> CreateUserBookingAsync(string userId, CreateUserBookingDto dto);
        // ✅ اليوزر يشوف حجوزاته
        Task<IEnumerable<UserBookingResponseDto>> GetUserBookingsAsync(
            string userId,
            string? timeFilter,
            int pageIndex = 0,
            int pageSize = 10);

        // ✅ اليوزر يكنسل حجزه
        Task CancelUserBookingAsync(string userId, Guid bookingId);

        // ✅ المواعيد المتاحة للدكتور
        Task<IEnumerable<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid doctorId,
            DateTime date);
    }
}
