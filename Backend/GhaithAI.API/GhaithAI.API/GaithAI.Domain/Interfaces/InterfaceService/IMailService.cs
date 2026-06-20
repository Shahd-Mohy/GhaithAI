using GhaithAI.API.GaithAI.Application.DTOs.Mail;

namespace GhaithAI.API.Services.Interfaces
{
    /// <summary>
    /// Service interface for sending emails via SMTP.
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// Sends an email asynchronously using the configured SMTP settings.
        /// </summary>
        /// <param name="mailRequest">The mail request containing recipient, subject, and body.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        /// <exception cref="System.Net.Mail.SmtpException">Thrown if SMTP connection or send fails.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if mailRequest is null.</exception>
        Task SendEmailAsync(MailRequest mailRequest);
    }
}
