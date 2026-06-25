using GhaithAI.GaithAI.Application.DTOs.Payment;
using GhaithAI.GaithAI.Domain.Entities;
using GhaithAI.GaithAI.Domain.Enums;
using IStripeClientContract = GhaithAI.GaithAI.Domain.Interfaces.InterfaceService.IStripeClient;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace GhaithAI.GaithAI.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IStripeClientContract _stripeClient;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public PaymentService(
            IStripeClientContract stripeClient,
            ApplicationDbContext context,
            IConfiguration config)
        {
            _stripeClient = stripeClient;
            _context = context;
            _config = config;
        }

        // ─── InitiateAsync ────────────────────────────────────────────────────
        public async Task<PaymentInitiatedResponseDto> InitiateAsync(Guid bookingId, string patientId)
        {
            // 1. جيب الـ Booking مع الـ Doctor
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

            // 2. جيب السعر من DoctorServiceSetting
            var serviceSetting = await _context.DoctorServiceSettings
                .FirstOrDefaultAsync(s => s.DoctorId == booking.DoctorId && s.IsActive)
                ?? throw new KeyNotFoundException($"No active service setting for doctor {booking.DoctorId}.");

            var amount = serviceSetting.FeePerSession;

            // 3. تأكد مفيش payment Paid موجودة
            var alreadyPaid = await _context.Payments
                .AnyAsync(p => p.BookingId == bookingId && p.Status == PaymentStatus.Paid);

            if (alreadyPaid)
                throw new InvalidOperationException("This booking is already paid.");

            // 4. بناء الـ URLs
            var frontendUrl = _config["FrontendUrl"]!.TrimEnd('/');
            var successUrl = $"{frontendUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl = $"{frontendUrl}/payment/cancel?bookingId={bookingId}";

            var patientEmail = booking.Patient?.Email ?? string.Empty;

            // 5. إنشاء Stripe Checkout Session
            var stripeResult = await _stripeClient.CreateCheckoutSessionAsync(
                amount: amount,
                currency: "usd",
                bookingId: bookingId.ToString(),
                patientEmail: patientEmail,
                successUrl: successUrl,
                cancelUrl: cancelUrl
            );

            // 6. حفظ Payment في الـ DB
            var payment = new Payment
            {
                BookingId = bookingId,
                PatientId = patientId,
                Amount = amount,
                Currency = "usd",
                Status = PaymentStatus.Pending,
                StripeSessionId = stripeResult.SessionId,
                StripePaymentIntentId = null,
                PaidAt = null,
                FailureReason = null
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentInitiatedResponseDto
            {
                PaymentId = payment.Id,
                CheckoutUrl = stripeResult.CheckoutUrl,
                StripeSessionId = stripeResult.SessionId
            };
        }

        // ─── ConfirmPaymentAsync ──────────────────────────────────────────────
        // الـ Frontend بيكلمه بعد redirect من Stripe بالـ session_id
        public async Task<PaymentStatusDto> ConfirmPaymentAsync(string sessionId)
        {
            // 1. تحقق من Stripe إن الدفع اتم فعلاً
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId)
                ?? throw new KeyNotFoundException("Payment not found.");

            // 2. حدّث الـ DB بناءً على حالة Stripe الفعلية
            switch (session.PaymentStatus)
            {
                case "paid":
                    payment.Status = PaymentStatus.Paid;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.StripePaymentIntentId = session.PaymentIntentId;
                    payment.FailureReason = null;
                    break;

                case "unpaid" when session.Status == "expired":
                    payment.Status = PaymentStatus.Cancelled;
                    payment.FailureReason = "Session expired.";
                    break;

                default:
                    payment.Status = PaymentStatus.Failed;
                    payment.FailureReason = $"Unexpected status: {session.PaymentStatus}";
                    break;
            }

            await _context.SaveChangesAsync();
            return MapToStatusDto(payment);
        }

        // ─── MockConfirmAsync (Testing فقط) ──────────────────────────────────
        public async Task<PaymentStatusDto> MockConfirmAsync(string sessionId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId)
                ?? throw new KeyNotFoundException($"No payment found for session {sessionId}.");

            payment.Status = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.StripePaymentIntentId = $"pi_mock_{Guid.NewGuid():N}";

            await _context.SaveChangesAsync();
            return MapToStatusDto(payment);
        }

        // ─── GetStatusByBookingIdAsync ────────────────────────────────────────
        public async Task<PaymentStatusDto> GetStatusByBookingIdAsync(Guid bookingId)
        {
            var payment = await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"No payment found for booking {bookingId}.");

            return MapToStatusDto(payment);
        }

        // ─── Helper ───────────────────────────────────────────────────────────
        private static PaymentStatusDto MapToStatusDto(Payment p) => new()
        {
            PaymentId = p.Id,
            BookingId = p.BookingId,
            Status = p.Status.ToString(),
            Amount = p.Amount,
            Currency = p.Currency,
            PaidAt = p.PaidAt
        };
    }
}