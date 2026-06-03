namespace GhaithAI.API.DTOs.Auth
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string ProfilePicture { get; set; }

        public DateTime Expiration { get; set; }
    }
}