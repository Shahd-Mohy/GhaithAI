
namespace GhaithAI.API.GaithAI.API.Configurations
{
    public class LangflowSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string FlowId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string AgentNodeId { get; set; } = string.Empty;
        public string ConversationHistoryPlaceholder { get; set; } = "CONVERSATION_HISTORY_PLACEHOLDER";
        public string UserContextPlaceholder { get; set; } = "USER_CONTEXT_PLACEHOLDER";
        public string MoodContextPlaceholder { get; set; } = "MOOD_CONTEXT_PLACEHOLDER";
    }
}
