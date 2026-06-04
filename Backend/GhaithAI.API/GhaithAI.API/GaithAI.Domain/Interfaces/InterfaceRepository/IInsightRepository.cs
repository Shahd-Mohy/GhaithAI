namespace GhaithAI.API.Repositories.Interfaces
{
    public interface IInsightRepository
    {

        Task<List<MoodLog>> GetMoodLogsInRangeAsync(string userId, DateTime from, DateTime to);
    }
}
