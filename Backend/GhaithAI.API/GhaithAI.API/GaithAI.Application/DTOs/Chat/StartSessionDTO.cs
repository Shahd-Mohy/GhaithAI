namespace GhaithAI.API.DTOs.Chat
{
    public class StartSessionDTO
    {
        /// <summary>
        /// Whether the AI should remember previous sessions.
        /// Only effective if the user has MemoryEnabled = true on their profile.
        /// </summary>
        public bool MemoryEnabled { get; set; } = false;

        /// <summary>Optional user-defined label for this session.</summary>
        public string? Title { get; set; }
    }
}
