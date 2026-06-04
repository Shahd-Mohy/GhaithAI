namespace GhaithAI.API.GaithAI.Domain.Exceptions
{
    /// <summary>
    /// Raised when a user attempts to start an AI chat session without accepting the required AI chat terms and conditions.
    /// This represents a legal/medical consent requirement: users must explicitly opt-in to AI-assisted interactions.
    /// Maps to HTTP 400 Bad Request response.
    /// </summary>
    public class AiConsentRequiredException : Exception
    {
        public AiConsentRequiredException()
            : base("User must accept AI chat terms and conditions before starting a session. Please update your consent settings in account preferences.")
        {
        }

        public AiConsentRequiredException(string message)
            : base(message)
        {
        }

        public AiConsentRequiredException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
