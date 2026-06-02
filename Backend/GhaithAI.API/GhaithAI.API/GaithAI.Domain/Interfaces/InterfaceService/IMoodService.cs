using GhaithAI.API.DTOs.Mood;
using GhaithAI.API.GaithAI.Application.DTOs.Mood;

namespace GhaithAI.API.Services.Interfaces
{
    public interface IMoodService
    {
        Task<MoodLogCreatedDTO> LogMoodAsync(string userId, CreateMoodLogDTO dto);

        Task<(List<MoodHistoryDTO> Items, int TotalCount)> GetHistoryAsync(
            string userId,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize);

        Task<List<CalendarDayDTO>> GetCalendarAsync(string userId, int year, int month);

        Task<MoodStatisticsDTO> GetStatisticsAsync(string userId, string period);

        Task<MoodHistoryDTO?> GetByIdAsync(string userId, Guid moodLogId);

        Task<bool> UpdateAsync(string userId, Guid moodLogId, UpdateMoodLogDTO dto);

        Task<byte[]> ExportCsvAsync(string userId);
    }
}
