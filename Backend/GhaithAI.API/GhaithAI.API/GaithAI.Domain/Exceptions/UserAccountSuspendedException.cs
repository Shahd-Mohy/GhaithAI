namespace GhaithAI.API.GaithAI.Domain.Exceptions
{
    /// <summary>
    /// Raised when a user attempts to perform an action while their account is inactive.
    /// This represents a business rule violation: only active users may initiate AI chat sessions.
    /// Maps to HTTP 403 Forbidden response.
    /// </summary>
    public class UserAccountSuspendedException : Exception
    {
        public UserAccountSuspendedException()
            : base("User account is not active. Please contact support to reactivate your account.")
        {
        }

        public UserAccountSuspendedException(string message)
            : base(message)
        {
        }

        public UserAccountSuspendedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
