using GhaithAI.GaithAI.Application.DTOs.SessionNote;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;

namespace GhaithAI.GaithAI.Application.Services.Class
{
    public class SessionNoteService : ISessionNoteService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionNoteService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> AddNoteAsync(Guid clinicalSessionId, AddNoteDto dto)
        {
            var sessionExists = await _unitOfWork.ClinicalSession
                .GetAllQueryableNoTracking()
                .AnyAsync(s => s.Id == clinicalSessionId);

            if (!sessionExists)
                throw new KeyNotFoundException("Clinical session not found.");

            var note = new SessionNote
            {
                Id = Guid.NewGuid(),
                ClinicalSessionId = clinicalSessionId,
                Content = dto.Content,
                NoteType = dto.NoteType
            };

            await _unitOfWork.SessionNote.AddAsync(note);
            await _unitOfWork.CompleteAsync();

            return note.Id;
        }

        public async Task<IEnumerable<NoteResponseDto>> GetNotesBySessionAsync(Guid clinicalSessionId)
        {
            var notes = await _unitOfWork.SessionNote
                .GetNotesBySessionAsync(clinicalSessionId);

            return notes.Select(n => new NoteResponseDto
            {
                Id = n.Id,
                ClinicalSessionId = n.ClinicalSessionId,
                Content = n.Content,
                NoteType = n.NoteType.ToString(),
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt
            });
        }

        public async Task<IEnumerable<DoctorNoteResponseDto>> GetNotesByDoctorAsync(string userId)
        {
            var doctorProfile = await _unitOfWork.DoctorProfile
                .GetAllQueryableNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId)
                ?? throw new KeyNotFoundException("Doctor profile not found.");

            var notes = await _unitOfWork.SessionNote.GetNotesByDoctorAsync(doctorProfile.Id);

            return notes.Select(n => new DoctorNoteResponseDto
            {
                Id = n.Id,
                ClinicalSessionId = n.ClinicalSessionId,
                Content = n.Content,
                NoteType = n.NoteType.ToString(),
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,

                PatientId = n.ClinicalSession.PatientId,
                PatientDisplayName = !string.IsNullOrWhiteSpace(n.ClinicalSession.Patient?.FullName)
                    ? n.ClinicalSession.Patient.FullName
                    : n.ClinicalSession.Patient?.Email,

                SessionStartedAt = n.ClinicalSession.StartedAt,
                SessionType = n.ClinicalSession.SessionType.ToString()
            });
        }

        public async Task UpdateNoteAsync(Guid clinicalSessionId, Guid noteId, UpdateNoteDto dto)
        {
            var note = await _unitOfWork.SessionNote
                .GetAllQueryableTracking()
                .FirstOrDefaultAsync(n => n.Id == noteId && n.ClinicalSessionId == clinicalSessionId)
                ?? throw new KeyNotFoundException("Note not found.");

            note.Content = dto.Content;
            note.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();
        }
    }
}
