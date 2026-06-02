namespace GhaithAI.API.GaithAI.Application.DTOs.Chat
{
    public class UserChatRequestDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
