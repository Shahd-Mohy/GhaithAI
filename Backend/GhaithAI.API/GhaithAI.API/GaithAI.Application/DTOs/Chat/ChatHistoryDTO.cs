using GhaithAI.API.GaithAI.Application.DTOs.Chat;

namespace GhaithAI.API.DTOs.Chat
{
    public class ChatHistoryDTO
    {
        public SessionDTO Session { get; set; } = null!;
        public IEnumerable<ChatMessageDTO> Messages { get; set; }
            = Enumerable.Empty<ChatMessageDTO>();
    }
}
