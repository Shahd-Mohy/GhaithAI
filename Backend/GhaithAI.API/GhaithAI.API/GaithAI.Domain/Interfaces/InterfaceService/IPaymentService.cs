using GhaithAI.GaithAI.Application.DTOs.Payment;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface IPaymentService
    {
        Task<PaymentInitiatedResponseDto> InitiateAsync(Guid bookingId, string patientId);
        Task<PaymentStatusDto> MockConfirmAsync(string sessionId);
        Task<PaymentStatusDto> GetStatusByBookingIdAsync(Guid bookingId);
    }
}