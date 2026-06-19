namespace GhaithAI.API.GaithAI.API.Configurations
{
    /// <summary>
    /// Strongly-typed configuration for SMTP mail settings.
    /// Maps to appsettings.json "MailSettings" section.
    /// </summary>
    public class MailSettings
    {
        /// <summary>Sender email address (e.g., "ghaithai.official@gmail.com").</summary>
        public string Mail { get; set; } = string.Empty;

        /// <summary>Display name for outgoing emails (e.g., "GhaithAI System").</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>SMTP password or app-specific password (for Gmail, use app password).</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>SMTP host (e.g., "smtp.gmail.com").</summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>SMTP port (typically 587 for TLS, 465 for SSL).</summary>
        public int Port { get; set; }
    }
}
