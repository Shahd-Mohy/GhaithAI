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
    }
}
