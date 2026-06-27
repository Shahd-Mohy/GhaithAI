using GhaithAI.GaithAI.Application.DTOs.ClinicalSession;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class ClinicalSessionService : IClinicalSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClinicalSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> StartSessionAsync(StartSessionDto dto)
        {
            var booking = await _unitOfWork.Booking.GetByIdAsync(dto.BookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            var session = new ClinicalSession
            {
                Id = Guid.NewGuid(),
                BookingId = dto.BookingId,
                DoctorId = booking.DoctorId,
                PatientId = booking.PatientId,
                StartedAt = DateTime.UtcNow,
                Status = ClinicalSessionStatus.InProgress,
                SessionType = dto.SessionType,
                Provider = dto.Provider,
                ChiefComplaint = dto.ChiefComplaint,
                VideoRoomId = dto.VideoRoomId,
                VideoRoomUrl = dto.VideoRoomUrl
            };

            await _unitOfWork.ClinicalSession.AddAsync(session);
            await _unitOfWork.CompleteAsync();

            return session.Id;
        }

        public async Task<SessionResponseDto> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _unitOfWork.ClinicalSession
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException("Session not found.");

            return MapToDto(session);
        }

        public async Task<SessionResponseDto> GetSessionByBookingIdAsync(Guid bookingId)
        {
            var session = await _unitOfWork.ClinicalSession
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(s => s.BookingId == bookingId)
                ?? throw new KeyNotFoundException("Session not started for this booking yet.");

            return MapToDto(session);
        }

        public async Task EndSessionAsync(Guid sessionId)
        {
            var session = await _unitOfWork.ClinicalSession
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(s => s.Id == sessionId)
                ?? throw new KeyNotFoundException("Session not found.");

            if (session.Status == ClinicalSessionStatus.Completed)
                throw new InvalidOperationException("Session is already completed.");

            session.EndedAt = DateTime.UtcNow;
            session.Status = ClinicalSessionStatus.Completed;
            session.DurationMinutes = (int)Math.Round((session.EndedAt.Value - session.StartedAt).TotalMinutes);

            var booking = await _unitOfWork.Booking
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(b => b.Id == session.BookingId)
                ?? throw new KeyNotFoundException("Associated booking not found.");

            booking.Status = BookingStatus.Completed;

            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<SessionResponseDto>> GetDoctorSessionsAsync(
            string doctorUserId, ClinicalSessionStatus? status, DateTime? date)
        {
            var doctorProfile = await _unitOfWork.DoctorProfile
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == doctorUserId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var sessions = await _unitOfWork.ClinicalSession
                .GetSessionsByDoctorAsync(doctorProfile.Id, status, date);

            return sessions.Select(MapToDto);
        }

        public async Task<IEnumerable<SessionResponseDto>> GetPatientSessionsAsync(string patientId)
        {
            var sessions = await _unitOfWork.ClinicalSession
                .GetSessionsByPatientAsync(patientId);

            return sessions.Select(MapToDto);
        }

        private static SessionResponseDto MapToDto(ClinicalSession session) => new()
        {
            Id = session.Id,
            BookingId = session.BookingId,
            DoctorId = session.DoctorId,
            PatientId = session.PatientId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            DurationMinutes = session.DurationMinutes,
            Status = session.Status.ToString(),
            SessionType = session.SessionType.ToString(),
            ChiefComplaint = session.ChiefComplaint,
            Provider = session.Provider,
            VideoRoomId = session.VideoRoomId,
            VideoRoomUrl = session.VideoRoomUrl
        };
    }
}