using GhaithAI.API.DTOs.Journal;
using GhaithAI.API.GaithAI.Application.DTOs.Journal;
using GhaithAI.API.Models;
using GhaithAI.API.Repositories.Interfaces;
using GhaithAI.API.Services.Interfaces;

namespace GhaithAI.API.Services.Class
{
    public class JournalService : IJournalService
    {
        private readonly IJournalRepository _journalRepository;

        public JournalService(IJournalRepository journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task<JournalCreatedDTO> CreateAsync(string userId, CreateJournalDTO dto)
        {
            var entry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title?.Trim(),
                Content = dto.Content.Trim(),
                PromptType = string.IsNullOrWhiteSpace(dto.PromptType) ? "free" : dto.PromptType.Trim(),
                MoodBefore = dto.MoodBefore?.Trim() ?? string.Empty,
                MoodAfter = string.Empty,
                Tags = NormaliseTags(dto.Tags),
                WordCount = CountWords(dto.Content),
                IsDeleted = false
            };

            await _journalRepository.AddAsync(entry);
            await _journalRepository.SaveChangesAsync();

            return new JournalCreatedDTO
            {
                JournalId = entry.Id,
                CreatedAt = entry.CreatedAt,
                WordCount = entry.WordCount
            };
        }

        public async Task<(List<JournalDTO> Items, int TotalCount)> GetAllAsync(
            string userId, string? search, int page, int pageSize)
        {
            var (entries, total) = await _journalRepository.GetByUserIdAsync(userId, search, page, pageSize);
            return (entries.Select(MapToDTO).ToList(), total);
        }

        public async Task<JournalDTO?> GetByIdAsync(string userId, Guid id)
        {
            var belongs = await _journalRepository.BelongsToUserAsync(id, userId);
            if (!belongs) return null;

            var entry = await _journalRepository.GetByIdAsync((object)id);
            return entry == null ? null : MapToDTO(entry);
        }

        public async Task<bool> UpdateAsync(string userId, Guid id, UpdateJournalDTO dto)
        {
            var belongs = await _journalRepository.BelongsToUserAsync(id, userId);
            if (!belongs) return false;

            var entry = await _journalRepository.GetByIdAsync((object)id);
            if (entry == null) return false;

            if (dto.Title != null) entry.Title = dto.Title.Trim();
            if (dto.Content != null)
            {
                entry.Content = dto.Content.Trim();
                entry.WordCount = CountWords(entry.Content);
            }
            if (dto.Tags != null) entry.Tags = NormaliseTags(dto.Tags);

            _journalRepository.Update(entry);
            await _journalRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string userId, Guid id)
        {
            var belongs = await _journalRepository.BelongsToUserAsync(id, userId);
            if (!belongs) return false;

            await _journalRepository.SoftDeleteAsync(id);
            await _journalRepository.SaveChangesAsync();
            return true;
        }

        private static JournalDTO MapToDTO(JournalEntry e) => new()
        {
            JournalId = e.Id,
            Title = e.Title,
            Content = e.Content,
            ContentPreview = e.Content.Length > 150 ? e.Content[..150].TrimEnd() + "…" : e.Content,
            PromptType = e.PromptType,
            MoodBefore = e.MoodBefore,
            Tags = ParseTags(e.Tags),
            WordCount = e.WordCount,
            Date = e.CreatedAt.ToString("yyyy-MM-dd"),
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt ?? DateTime.UtcNow
        };

        private static int CountWords(string? text) =>
            string.IsNullOrWhiteSpace(text) ? 0 :
            text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        private static List<string> ParseTags(string? tags) =>
            string.IsNullOrWhiteSpace(tags) ? new() :
            tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t)).ToList();

        private static string? NormaliseTags(string? tags)
        {
            if (string.IsNullOrWhiteSpace(tags)) return null;
            var result = string.Join(",", tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower()).Where(t => !string.IsNullOrEmpty(t)));
            return string.IsNullOrEmpty(result) ? null : result;
        }
    }
}
