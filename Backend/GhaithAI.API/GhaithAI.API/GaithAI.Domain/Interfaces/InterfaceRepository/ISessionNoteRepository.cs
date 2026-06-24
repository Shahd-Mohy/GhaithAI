namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface ISessionNoteRepository : IGenericRepository<SessionNote>
    {
        Task<IEnumerable<SessionNote>> GetNotesBySessionAsync(Guid clinicalSessionId);
        Task<List<SessionNote>> GetNotesByDoctorAsync(Guid doctorId);
    }
}
