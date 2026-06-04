using GhaithAI.API.Constants;
using GhaithAI.API.DTOs.Insight;
using GhaithAI.API.ViewModels;
using GhaithAI.GaithAI.Application.DTOs.Insight;

namespace GhaithAI.API.Services.Class
{
    public class InsightService : IInsightService
    {
        private readonly IInsightRepository _insightRepository;

        public InsightService(IInsightRepository insightRepository)
        {
            _insightRepository = insightRepository;
        }

        public async Task<DashboardViewModel> GetDashboardAsync(string userId, string displayName)
        {
            var today = DateTime.UtcNow.Date;
            var rangeStart = today.AddDays(-13);
            var rangeEnd = today.AddDays(1).AddTicks(-1);

            var logs = await _insightRepository.GetMoodLogsInRangeAsync(userId, rangeStart, rangeEnd);

            var weekStart = today.AddDays(-6); 
            var currentWeekLogs = logs.Where(l => l.LoggedAt.Date >= weekStart).ToList();
            var previousWeekLogs = logs.Where(l => l.LoggedAt.Date < weekStart).ToList();

            return new DashboardViewModel
            {
                DisplayName = displayName,
                TodayLabel = DateTime.UtcNow.ToString("dddd, MMMM d"),
                WeeklySummary = BuildWeeklySummary(currentWeekLogs, weekStart, today),
                PersonalInsights = BuildPersonalInsights(currentWeekLogs, previousWeekLogs),
                TodaysSuggestion = BuildTodaysSuggestion(currentWeekLogs, today)
            };
        }

        private static WeeklyInsightDTO BuildWeeklySummary(
            List<MoodLog> logs, DateTime weekStart, DateTime today)
        {
            var dailyMoods = new List<DailyMoodDTO>();

            for (int i = 0; i <= 6; i++)
            {
                var date = weekStart.AddDays(i);
                if (date > today) break;

                var dayLogs = logs.Where(l => l.LoggedAt.Date == date).ToList();
                decimal? score = dayLogs.Any()
                    ? Math.Round((decimal)dayLogs.Average(l => l.MoodScore), 1)
                    : null;

                dailyMoods.Add(new DailyMoodDTO
                {
                    DayLabel = date.ToString("ddd"), 
                    Date = date.ToString("yyyy-MM-dd"),
                    MoodScore = score
                });
            }

            var loggedDays = dailyMoods.Count(d => d.MoodScore.HasValue);
            var avg = dailyMoods.Where(d => d.MoodScore.HasValue).Any()
                ? Math.Round(dailyMoods.Where(d => d.MoodScore.HasValue)
                                       .Average(d => d.MoodScore!.Value), 1)
                : 0m;

            return new WeeklyInsightDTO
            {
                DailyMoods = dailyMoods,
                AvgMoodScore = avg,
                AvgMoodLabel = $"{avg} / 5",
                DaysLogged = loggedDays,
                DaysLoggedLabel = $"{loggedDays} / 7"
            };
        }


        private static List<PersonalInsightDTO> BuildPersonalInsights(
            List<MoodLog> currentWeek, List<MoodLog> previousWeek)
        {
            var insights = new List<PersonalInsightDTO>();

            var streakDays = CountStreak(currentWeek);
            if (streakDays >= 2)
                insights.Add(new PersonalInsightDTO
                {
                    Text = $"You've logged {streakDays} days in a row — great consistency!"
                });

            if (currentWeek.Any() && previousWeek.Any())
            {
                var currAvg = currentWeek.Average(l => l.MoodScore);
                var prevAvg = previousWeek.Average(l => l.MoodScore);
                var diff = currAvg - prevAvg;

                if (diff >= 0.5)
                    insights.Add(new PersonalInsightDTO
                    {
                        Text = "Your mood has been higher this week compared to last week."
                    });
                else if (diff <= -0.5)
                    insights.Add(new PersonalInsightDTO
                    {
                        Text = "Your mood dipped a bit this week — it's okay to have tough stretches."
                    });
            }

            var logsWithSleep = currentWeek.Where(l => l.SleepQuality > 0).ToList();
            if (logsWithSleep.Count >= 3)
            {
                var goodSleepMood = logsWithSleep.Where(l => l.SleepQuality >= 4)
                                                 .Select(l => (double)l.MoodScore)
                                                 .DefaultIfEmpty(0).Average();
                var poorSleepMood = logsWithSleep.Where(l => l.SleepQuality < 4)
                                                 .Select(l => (double)l.MoodScore)
                                                 .DefaultIfEmpty(0).Average();

                if (goodSleepMood - poorSleepMood >= 1.0)
                    insights.Add(new PersonalInsightDTO
                    {
                        Text = "Your mood tends to be higher on nights with good sleep."
                    });
            }

            var journalDays = currentWeek
                .Where(l => !string.IsNullOrWhiteSpace(l.Notes))
                .Select(l => l.LoggedAt.Date)
                .Distinct()
                .Count();

            if (journalDays >= 2)
                insights.Add(new PersonalInsightDTO
                {
                    Text = "Writing in your journal seems to help you process your emotions."
                });

            var topEmotion = GetTopEmotion(currentWeek);
            if (!string.IsNullOrEmpty(topEmotion))
                insights.Add(new PersonalInsightDTO
                {
                    Text = $"Your most frequent emotion this week was \"{topEmotion}\"."
                });

            return insights.Take(3).ToList();
        }


        private static RecommendationDTO BuildTodaysSuggestion(
            List<MoodLog> currentWeek, DateTime today)
        {
            var todayLogs = currentWeek.Where(l => l.LoggedAt.Date == today).ToList();

            if (todayLogs.Any())
            {
                var latestScore = todayLogs.OrderByDescending(l => l.LoggedAt).First().MoodScore;

                if (latestScore <= 2)
                    return new RecommendationDTO
                    {
                        Title = "Today's Suggestion",
                        Text = "It sounds like today is tough. A short breathing exercise can help you find calm in just 5 minutes.",
                        ActionLabel = "Start Breathing Exercise",
                        ActionRoute = "breathing"
                    };

                if (latestScore == 3)
                    return new RecommendationDTO
                    {
                        Title = "Today's Suggestion",
                        Text = "A good day to reflect — writing a few lines in your journal can help you see things more clearly.",
                        ActionLabel = "Open Journal",
                        ActionRoute = "journal"
                    };

                return new RecommendationDTO
                {
                    Title = "Today's Suggestion",
                    Text = "You're in a good space today! Explore a new psychoeducation article to build on that momentum.",
                    ActionLabel = "Learn Something",
                    ActionRoute = "learn"
                };
            }

            var avgThisWeek = currentWeek.Any()
                ? currentWeek.Average(l => l.MoodScore)
                : 0;

            if (avgThisWeek > 0 && avgThisWeek <= 2.5)
                return new RecommendationDTO
                {
                    Title = "Today's Suggestion",
                    Text = "Try a 5-minute breathing exercise to start your day with calm. Research shows it can reduce anxiety by up to 37%.",
                    ActionLabel = "Start Breathing Exercise",
                    ActionRoute = "breathing"
                };

            return new RecommendationDTO
            {
                Title = "Today's Suggestion",
                Text = "Take a moment to check in with yourself. Logging your mood takes less than 30 seconds and helps you spot patterns over time.",
                ActionLabel = null,
                ActionRoute = null
            };
        }

        private static int CountStreak(List<MoodLog> logs)
        {
            if (!logs.Any()) return 0;

            var days = logs.Select(l => l.LoggedAt.Date)
                           .Distinct()
                           .OrderByDescending(d => d)
                           .ToList();

            var today = DateTime.UtcNow.Date;
            if (days[0] < today.AddDays(-1)) return 0;

            int streak = 0;
            var expected = today;
            foreach (var d in days)
            {
                if (d == expected || d == expected.AddDays(-1))
                {
                    streak++;
                    expected = d.AddDays(-1);
                }
                else break;
            }
            return streak;
        }

        private static string? GetTopEmotion(List<MoodLog> logs)
        {
            return logs
                .Where(l => !string.IsNullOrWhiteSpace(l.EmotionTags))
                .SelectMany(l => EmotionTypes.Parse(l.EmotionTags))
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
        }
    }
}
