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

        private static readonly TimeZoneInfo EgyptTz =
            TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

        public MoodService(IMoodRepository moodRepository)
        {
            _moodRepository = moodRepository;
        }

        private static DateTime ToEgyptDate(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(utc, EgyptTz).Date;

        private static DateTime EgyptNow() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTz);

        // POST /api/mood/log
        public async Task<MoodLogCreatedDTO> LogMoodAsync(string userId, CreateMoodLogDTO dto)
        {
            var moodLog = new MoodLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MoodScore = dto.MoodScore,
                EmotionTags = dto.EmotionTags?.Trim(),
                StressLevel = dto.StressLevel,
                SleepQuality = dto.SleepQuality,
                Notes = dto.Notes?.Trim(),
                Source = dto.Source ?? "manual",
                LoggedAt = dto.LoggedAt ?? DateTime.UtcNow,
                IsDeleted = false
            };

            await _moodRepository.AddAsync(moodLog);
            await _moodRepository.SaveChangesAsync();

            return new MoodLogCreatedDTO
            {
                MoodLogId = moodLog.Id,
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

            return logs.Select(log =>
            {
                var egyptLocalDate = ToEgyptDate(log.LoggedAt);
                return new CalendarDayDTO
                {
                    MoodLogId = log.Id,
                    Date = egyptLocalDate.ToString("yyyy-MM-dd"),
                    MoodScore = log.MoodScore,
                    MoodLabel = MoodTypes.GetLabel(log.MoodScore),
                    MoodBadge = MoodTypes.GetBadge(log.MoodScore),
                    EmotionTags = EmotionTypes.Parse(log.EmotionTags),
                    Notes = log.Notes
                };
            }).ToList();
        }

        // GET /api/mood/statistics?period=
        public async Task<MoodStatisticsDTO> GetStatisticsAsync(string userId, string period)
        {
            var egyptNow = EgyptNow();
            var utcNow = DateTime.UtcNow;

            DateTime fromUtc;
            if (period.ToLower() == "week")
            {
                fromUtc = utcNow.AddDays(-7);
            }
            else
            {
                var egyptMonthStart = new DateTime(egyptNow.Year, egyptNow.Month, 1, 0, 0, 0);
                fromUtc = TimeZoneInfo.ConvertTimeToUtc(egyptMonthStart, EgyptTz);
            }

            var logs = await _moodRepository.GetAllByUserIdAsync(userId, fromUtc, utcNow);

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

            var avg = Math.Round((decimal)logs.Average(l => l.MoodScore), 1);

            var totalLogs = logs
                .Select(l => ToEgyptDate(l.LoggedAt))
                .Distinct()
                .Count();

            var streak = CalculateStreak(logs);

            var validEmotions = new HashSet<string>(EmotionTypes.ValidEmotions, StringComparer.OrdinalIgnoreCase);

            var allTags = logs
                .Where(l => !string.IsNullOrWhiteSpace(l.EmotionTags))
                .SelectMany(l => EmotionTypes.Parse(l.EmotionTags))
                .Where(t => validEmotions.Contains(t))
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();


            var last7From = utcNow.AddDays(-7);
            var prev7From = utcNow.AddDays(-14);
            var prev7To = utcNow.AddDays(-7);

            var last7Logs = await _moodRepository.GetAllByUserIdAsync(userId, last7From, utcNow);
            var prev7Logs = await _moodRepository.GetAllByUserIdAsync(userId, prev7From, prev7To);

            var last7Avg = last7Logs.Any() ? (decimal)last7Logs.Average(l => l.MoodScore) : avg;
            var prev7Avg = prev7Logs.Any() ? (decimal)prev7Logs.Average(l => l.MoodScore) : last7Avg;
            var change = Math.Round(last7Avg - prev7Avg, 1);

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

        /// <inheritdoc/>
        public async Task<AiMoodContextDto> GetLast7DaysMoodSummaryAsync(string userId)
        {
            var utcNow = DateTime.UtcNow;
            var from7 = utcNow.AddDays(-7);
            var from14 = utcNow.AddDays(-14);

            var last7Logs = await _moodRepository.GetAllByUserIdAsync(userId, from7, utcNow);
            var prev7Logs = await _moodRepository.GetAllByUserIdAsync(userId, from14, from7);

            if (!last7Logs.Any())
            {
                return new AiMoodContextDto
                {
                    LoggedDaysCount = 0,
                    AverageMoodScore = 0,
                    AverageMoodLabel = "No data",
                    MoodTrend = "Insufficient data",
                    ChangeFromPreviousWeek = 0,
                    DominantEmotion = "None",
                    DominantEmotionCount = 0,
                    EmotionPattern = new(),
                    StreakDays = 0
                };
            }

            var loggedDays = last7Logs.Select(l => ToEgyptDate(l.LoggedAt)).Distinct().Count();
            var last7Avg = Math.Round((decimal)last7Logs.Average(l => l.MoodScore), 1);
            var prev7Avg = prev7Logs.Any() ? Math.Round((decimal)prev7Logs.Average(l => l.MoodScore), 1) : last7Avg;

            var change = Math.Round(last7Avg - prev7Avg, 1);

            var trend = change switch
            {
                > 0.5m => "Improving",
                < -0.5m => "Declining",
                _ => "Stable"
            };

            var validEmotions = new HashSet<string>(EmotionTypes.ValidEmotions, StringComparer.OrdinalIgnoreCase);

            var emotionGroups = last7Logs
                .Where(l => !string.IsNullOrWhiteSpace(l.EmotionTags))
                .SelectMany(l => EmotionTypes.Parse(l.EmotionTags))
                .Where(t => validEmotions.Contains(t))
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ToList();

            var dominantGroup = emotionGroups.FirstOrDefault();
            var emotionPattern = emotionGroups.Select(g => g.Key).Take(5).ToList();

            var streak = CalculateStreak(last7Logs);

            return new AiMoodContextDto
            {
                LoggedDaysCount = loggedDays,
                AverageMoodScore = last7Avg,
                AverageMoodLabel = MoodTypes.GetLabel((int)Math.Round(last7Avg)),
                MoodTrend = trend,
                ChangeFromPreviousWeek = change,
                DominantEmotion = dominantGroup?.Key ?? "None",
                DominantEmotionCount = dominantGroup?.Count() ?? 0,
                EmotionPattern = emotionPattern,
                StreakDays = streak
            };
        }

        // GET /api/mood/{id}
        public async Task<MoodHistoryDTO?> GetByIdAsync(string userId, Guid moodLogId)
        {
            var belongs = await _moodRepository.BelongsToUserAsync(moodLogId, userId);
            if (!belongs) return null;
            var log = await _moodRepository.GetByIdAsync(moodLogId);
            return log == null ? null : MapToHistoryDTO(log);
        }

        // PUT /api/mood/{id}
        public async Task<bool> UpdateAsync(string userId, Guid moodLogId, UpdateMoodLogDTO dto)
        {
            var log = await _moodRepository.GetByIdAsync(moodLogId);

            if (log == null || log.UserId != userId) return false;

            if (dto.MoodScore.HasValue)
                log.MoodScore = dto.MoodScore.Value;

            if (dto.EmotionTags != null)
                log.EmotionTags = dto.EmotionTags.Trim();

            if (dto.Notes != null)
                log.Notes = dto.Notes.Trim();

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
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(log.LoggedAt, EgyptTz);
                sb.AppendLine(string.Join(",",
                    localTime.ToString("yyyy-MM-dd"),
                    localTime.ToString("HH:mm:ss"),
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
            var egyptLocalDate = ToEgyptDate(log.LoggedAt);
            return new MoodHistoryDTO
            {
                MoodLogId = log.Id,
                MoodScore = log.MoodScore,
                MoodLabel = MoodTypes.GetLabel(log.MoodScore),
                MoodBadge = MoodTypes.GetBadge(log.MoodScore),
                EmotionTags = EmotionTypes.Parse(log.EmotionTags),
                Date = egyptLocalDate.ToString("yyyy-MM-dd"),
                LoggedAt = log.LoggedAt,
                Notes = log.Notes
            };
        }

        private static int CalculateStreak(List<MoodLog> logs)
        {
            var distinctDates = logs
                .Select(l => ToEgyptDate(l.LoggedAt))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!distinctDates.Any()) return 0;

            var today = EgyptNow().Date;
            if (distinctDates[0] < today.AddDays(-1)) return 0;

            var streak = 0;
            var expected = distinctDates[0];

            foreach (var date in distinctDates)
            {
                if (date == expected) { streak++; expected = expected.AddDays(-1); }
                else break;
            }

            return streak;
        }
    }
}
