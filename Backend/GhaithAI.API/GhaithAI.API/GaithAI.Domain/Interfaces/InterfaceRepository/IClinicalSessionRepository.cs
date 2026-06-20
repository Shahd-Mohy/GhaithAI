namespace GhaithAI.GaithAI.Domain.Interfaces.InterfaceRepository
{
    public interface IClinicalSessionRepository : IGenericRepository<ClinicalSession>
    {
        Task<IEnumerable<ClinicalSession>> GetSessionsByDoctorAsync(Guid doctorId, ClinicalSessionStatus? status, DateTime? date);
        Task<IEnumerable<ClinicalSession>> GetSessionsByPatientAsync(string patientId);
    }
}
