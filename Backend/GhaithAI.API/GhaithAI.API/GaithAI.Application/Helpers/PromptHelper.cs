namespace GhaithAI.API.Helpers
{
    public static class PromptHelper
    {
        public static string BuildChatPrompt(
            string message)
        {
            return $"""
            You are a supportive AI assistant.

            User Message:
            {message}
            """;
        }
    }
}