namespace GhaithAI.API.DTOs.Chat
{
    public class ChatRequestDTO
    {
        public string Message { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
}
