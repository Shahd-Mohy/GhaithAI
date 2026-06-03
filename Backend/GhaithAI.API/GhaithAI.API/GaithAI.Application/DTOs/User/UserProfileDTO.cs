namespace GhaithAI.API.DTOs.User
{
    public class UserProfileDTO
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string CountryCode { get; set; }

        public string PreferredLanguage { get; set; }

        public bool MemoryEnabled { get; set; }

        public string ProfilePicture { get; set; }
    }
}