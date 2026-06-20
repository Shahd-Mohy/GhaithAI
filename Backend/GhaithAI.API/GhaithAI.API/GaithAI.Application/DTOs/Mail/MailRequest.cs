namespace GhaithAI.API.GaithAI.Application.DTOs.Mail
{
    /// <summary>
    /// Request DTO for sending an email message.
    /// </summary>
    public sealed class MailRequest
    {
        /// <summary>Recipient email address.</summary>
        public string ToEmail { get; set; } = string.Empty;

        /// <summary>Email subject line.</summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>Email body content (supports HTML).</summary>
        public string Body { get; set; } = string.Empty;
    }
}
