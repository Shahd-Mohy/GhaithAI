using GhaithAI.API.Constants;
using GhaithAI.API.DTOs.Mood;
using GhaithAI.API.GaithAI.Application.DTOs.Mood;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;
using GhaithAI.API.Services.Interfaces;
using System.Text;

namespace GhaithAI.API.Services.Class
{
    public class MoodService : IMoodService
    {
            private readonly IMoodRepository _moodRepository;

            public MoodService(IMoodRepository moodRepository)
            {
                _moodRepository = moodRepository;
            }

            // POST /api/mood/log
            public async Task<MoodLogCreatedDTO> LogMoodAsync(string userId, CreateMoodLogDTO dto)
            {
                var moodLog = new MoodLog
                {
                    MoodLogId = Guid.NewGuid(),
                    UserId = userId,
                    MoodScore = dto.MoodScore,
                    EmotionTags = dto.EmotionTags?.Trim(),
                    StressLevel = dto.StressLevel,
                    SleepQuality = dto.SleepQuality,
                    Notes = dto.Notes?.Trim(),
                    Source = dto.Source,
                    LoggedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _moodRepository.AddAsync(moodLog);
                await _moodRepository.SaveChangesAsync();

                return new MoodLogCreatedDTO
                {
                    MoodLogId = moodLog.MoodLogId,
                    LoggedAt = moodLog.LoggedAt,
                    MoodLabel = MoodTypes.GetLabel(moodLog.MoodScore)
                };
            }

            // GET /api/mood/history
            public async Task<(List<MoodHistoryDTO> Items, int TotalCount)> GetHistoryAsync(
                string userId, DateTime? from, DateTime? to, int page, int pageSize)
            {
                var (logs, total) = await _moodRepository.GetByUserIdAsync(userId, from, to, page, pageSize);

                var items = logs.Select(MapToHistoryDTO).ToList();

                return (items, total);
            }

            // GET /api/mood/calendar?year=&month=
            public async Task<List<CalendarDayDTO>> GetCalendarAsync(string userId, int year, int month)
            {
                var logs = await _moodRepository.GetCalendarAsync(userId, year, month);

                return logs.Select(log => new CalendarDayDTO
                {
                    Date = log.LoggedAt.ToString("yyyy-MM-dd"),
                    MoodScore = log.MoodScore,
                    MoodLabel = MoodTypes.GetLabel(log.MoodScore),
                    MoodBadge = MoodTypes.GetBadge(log.MoodScore),
                    EmotionTags = EmotionTypes.Parse(log.EmotionTags)
                }).ToList();
            }

            // GET /api/mood/statistics?period=
            public async Task<MoodStatisticsDTO> GetStatisticsAsync(string userId, string period)
            {
                var now = DateTime.UtcNow;
                var from = period.ToLower() == "week"
                    ? now.AddDays(-7)
                    : now.AddDays(-30); 

                var logs = await _moodRepository.GetAllByUserIdAsync(userId, from, now);

                if (!logs.Any())
                {
                    return new MoodStatisticsDTO
                    {
                        Period = period,
                        AvgMoodScore = 0,
                        TotalLogs = 0,
                        StreakDays = 0,
                        TopEmotion = "None",
                        TopEmotionCount = 0,
                        ChangeFromLastWeek = 0
                    };
                }

                // Average mood score 
                var avg = Math.Round((decimal)logs.Average(l => l.MoodScore), 1);

                var totalLogs = logs.Select(l => l.LoggedAt.Date).Distinct().Count();

                // Calculate streak
                var streak = CalculateStreak(logs);

                // Top emotion
                var allTags = logs
                    .Where(l => !string.IsNullOrWhiteSpace(l.EmotionTags))
                    .SelectMany(l => EmotionTypes.Parse(l.EmotionTags))
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                var lastWeekFrom = now.AddDays(-14);
                var lastWeekTo = now.AddDays(-7);
                var lastWeekLogs = await _moodRepository.GetAllByUserIdAsync(userId, lastWeekFrom, lastWeekTo);
                var lastWeekAvg = lastWeekLogs.Any()
                    ? (decimal)lastWeekLogs.Average(l => l.MoodScore)
                    : avg;
                var change = Math.Round(avg - lastWeekAvg, 1);

                return new MoodStatisticsDTO
                {
                    AvgMoodScore = avg,
                    TotalLogs = totalLogs,
                    StreakDays = streak,
                    TopEmotion = allTags?.Key ?? "None",
                    TopEmotionCount = allTags?.Count() ?? 0,
                    ChangeFromLastWeek = change,
                    Period = period
                };
            }

            // GET /api/mood/{id}
            public async Task<MoodHistoryDTO?> GetByIdAsync(string userId, Guid moodLogId)
            {
                // Security check
                var belongs = await _moodRepository.BelongsToUserAsync(moodLogId, userId);
                if (!belongs) return null;

                var log = await _moodRepository.GetByIdAsync(moodLogId);
                return log == null ? null : MapToHistoryDTO(log);
            }

            // PUT /api/mood/{id}
            public async Task<bool> UpdateAsync(string userId, Guid moodLogId, UpdateMoodLogDTO dto)
            {
                var belongs = await _moodRepository.BelongsToUserAsync(moodLogId, userId);
                if (!belongs) return false;

                var log = await _moodRepository.GetByIdAsync(moodLogId);
                if (log == null) return false;

                if (dto.MoodScore.HasValue)
                    log.MoodScore = dto.MoodScore.Value;

                if (dto.EmotionTags != null)
                    log.EmotionTags = dto.EmotionTags.Trim();

                _moodRepository.Update(log);
                await _moodRepository.SaveChangesAsync();
                return true;
            }

            // GET /api/mood/export
            public async Task<byte[]> ExportCsvAsync(string userId)
            {
                var from = DateTime.UtcNow.AddYears(-10);
                var logs = await _moodRepository.GetAllByUserIdAsync(userId, from, DateTime.UtcNow);

                var sb = new StringBuilder();

                sb.AppendLine("Date,Time,MoodScore,MoodLabel,EmotionTags,StressLevel,SleepQuality,Notes");

                foreach (var log in logs)
                {
                    sb.AppendLine(string.Join(",",
                        log.LoggedAt.ToString("yyyy-MM-dd"),
                        log.LoggedAt.ToString("HH:mm:ss"),
                        log.MoodScore,
                        MoodTypes.GetLabel(log.MoodScore),
                        $"\"{log.EmotionTags ?? ""}\"", 
                        log.StressLevel,
                        log.SleepQuality,
                        $"\"{log.Notes ?? ""}\""));
                }

                return Encoding.UTF8.GetBytes(sb.ToString());
            }

            
            // helpers

            private static MoodHistoryDTO MapToHistoryDTO(MoodLog log)
            {
                return new MoodHistoryDTO
                {
                    MoodLogId = log.MoodLogId,
                    MoodScore = log.MoodScore,
                    MoodLabel = MoodTypes.GetLabel(log.MoodScore),
                    MoodBadge = MoodTypes.GetBadge(log.MoodScore),
                    EmotionTags = EmotionTypes.Parse(log.EmotionTags),
                    Date = log.LoggedAt.ToString("yyyy-MM-dd"),
                    LoggedAt = log.LoggedAt
                };
            }

            private static int CalculateStreak(List<MoodLog> logs)
            {
                var distinctDates = logs
                    .Select(l => l.LoggedAt.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                if (!distinctDates.Any()) return 0;

                // streak is broken
                var today = DateTime.UtcNow.Date;
                if (distinctDates[0] < today.AddDays(-1)) return 0;

                var streak = 0;
                var expected = today;

                foreach (var date in distinctDates)
                {
                    if (date == expected || date == expected.AddDays(-1))
                    {
                        streak++;
                        expected = date.AddDays(-1);
                    }
                    else break;
                }

                return streak;
            }
        }
}
