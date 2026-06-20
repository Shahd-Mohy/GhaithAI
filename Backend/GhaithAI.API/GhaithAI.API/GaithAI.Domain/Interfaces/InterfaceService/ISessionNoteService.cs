using GhaithAI.GaithAI.Application.DTOs.SessionNote;

namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceService
{
    public interface ISessionNoteService
    {
        Task<Guid> AddNoteAsync(Guid clinicalSessionId, AddNoteDto dto);
        Task<IEnumerable<NoteResponseDto>> GetNotesBySessionAsync(Guid clinicalSessionId);
        Task UpdateNoteAsync(Guid clinicalSessionId, Guid noteId, UpdateNoteDto dto);
    }
}
