namespace GhaithAI.API.DTOs.User
{
    public class UpdateProfileDTO
    {
        public string FullName { get; set; }

        public string PreferredLanguage { get; set; }

        public bool MemoryEnabled { get; set; }
    }
}