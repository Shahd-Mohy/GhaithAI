using GhaithAI.GaithAI.Application.DTOs.Payment;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class MockPaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public MockPaymentService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<PaymentInitiatedResponseDto> InitiateAsync(
            Guid bookingId, string patientId)
        {
            // تأكد إن الـ Booking موجود
            var booking = await _unitOfWork.Booking
                .GetAllQueryableNoTracking()
                .Include(b => b.Doctor)
                .ThenInclude(d => d.ServiceSetting)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            // لو فيه payment موجود بالفعل وهو Pending، رجّعه بدل ما تعمل جديد
            var existing = await _unitOfWork.Payment
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.BookingId == bookingId &&
                    p.Status == PaymentStatus.Pending);

            if (existing != null)
            {
                var frontendBase = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";
                return new PaymentInitiatedResponseDto
                {
                    PaymentId = existing.Id,
                    StripeSessionId = existing.StripeSessionId,
                    CheckoutUrl = $"{frontendBase}/payment/mock?sessionId={existing.StripeSessionId}"
                };
            }

            // عمل Mock Session ID
            var mockSessionId = $"MOCK-{Guid.NewGuid():N}";

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = bookingId,
                PatientId = patientId,
                Amount = booking.Doctor?.ServiceSetting?.FeePerSession ?? 0,
                Currency = "usd",
                Status = PaymentStatus.Pending,
                StripeSessionId = mockSessionId
            };

            await _unitOfWork.Payment.AddAsync(payment);
            await _unitOfWork.CompleteAsync();

            var baseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:4200";

            return new PaymentInitiatedResponseDto
            {
                PaymentId = payment.Id,
                StripeSessionId = mockSessionId,
                CheckoutUrl = $"{baseUrl}/payment/mock?sessionId={mockSessionId}"
            };
        }

        public async Task<PaymentStatusDto> MockConfirmAsync(string sessionId)
        {
            var payment = await _unitOfWork.Payment
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId)
                ?? throw new KeyNotFoundException("Payment session not found.");

            if (payment.Status == PaymentStatus.Paid)
                throw new InvalidOperationException("Payment already confirmed.");

            // تأكيد الدفع
            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.StripePaymentIntentId = $"MOCK-PI-{Guid.NewGuid():N}";

            // تأكيد الحجز
            var booking = await _unitOfWork.Booking
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            booking.Status = BookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;
            booking.ConfirmedBy = payment.PatientId;

            await _unitOfWork.CompleteAsync();

            // TODO: NotificationService.SendAsync للدكتور والمريض

            return MapToStatusDto(payment);
        }

        public async Task<PaymentStatusDto> GetStatusByBookingIdAsync(Guid bookingId)
        {
            var payment = await _unitOfWork.Payment
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(p => p.BookingId == bookingId)
                ?? throw new KeyNotFoundException("No payment found for this booking.");

            return MapToStatusDto(payment);
        }

        private static PaymentStatusDto MapToStatusDto(Payment payment) => new()
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaidAt = payment.PaidAt
        };
    }
}