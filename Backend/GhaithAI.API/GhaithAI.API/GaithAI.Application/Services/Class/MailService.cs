using GhaithAI.API.GaithAI.API.Configurations;
using GhaithAI.API.GaithAI.Application.DTOs.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GhaithAI.API.Services.Class
{
    /// <summary>
    /// SMTP email service implementation using MailKit and MimeKit.
    /// Provides asynchronous, secure email sending with comprehensive error handling.
    /// </summary>
    public sealed class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<MailService> _logger;

        public MailService(
            IOptions<MailSettings> mailSettings,
            ILogger<MailService> logger)
        {
            _mailSettings = mailSettings.Value ?? throw new ArgumentNullException(nameof(mailSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            // ── 1. Validate input
            if (mailRequest is null)
                throw new ArgumentNullException(nameof(mailRequest));

            if (string.IsNullOrWhiteSpace(mailRequest.ToEmail))
                throw new ArgumentException("Recipient email address cannot be empty.", nameof(mailRequest));

            if (string.IsNullOrWhiteSpace(mailRequest.Subject))
                throw new ArgumentException("Email subject cannot be empty.", nameof(mailRequest));

            if (string.IsNullOrWhiteSpace(mailRequest.Body))
                throw new ArgumentException("Email body cannot be empty.", nameof(mailRequest));

            // ── 2. Validate configuration
            if (string.IsNullOrWhiteSpace(_mailSettings.Mail) ||
                string.IsNullOrWhiteSpace(_mailSettings.Password) ||
                string.IsNullOrWhiteSpace(_mailSettings.Host))
            {
                _logger.LogError("Mail service configuration is incomplete. Check MailSettings in appsettings.json.");
                throw new InvalidOperationException("Mail service is not properly configured.");
            }

            // ── 3. Build email message
            var email = BuildMimeMessage(mailRequest);

            // ── 4. Send via SMTP
            using var client = new SmtpClient();
            try
            {
                _logger.LogInformation(
                    "Connecting to SMTP server {Host}:{Port}",
                    _mailSettings.Host, _mailSettings.Port);

                // Connect asynchronously with StartTls
                await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);

                // Authenticate asynchronously
                await client.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                // Send email asynchronously
                _logger.LogInformation(
                    "Sending email to {ToEmail} with subject '{Subject}'",
                    mailRequest.ToEmail, mailRequest.Subject);

                await client.SendAsync(email);

                _logger.LogInformation(
                    "Email successfully sent to {ToEmail}",
                    mailRequest.ToEmail);
            }
            catch (SmtpCommandException smtpEx)
            {
                _logger.LogError(smtpEx,
                    "SMTP command failed while sending email to {ToEmail}. Status Code: {StatusCode}",
                    mailRequest.ToEmail, smtpEx.StatusCode);

                throw new InvalidOperationException(
                    $"Failed to send email: {smtpEx.Message}", smtpEx);
            }
            catch (SmtpProtocolException protocolEx)
            {
                _logger.LogError(protocolEx,
                    "SMTP protocol error while sending email to {ToEmail}",
                    mailRequest.ToEmail);

                throw new InvalidOperationException(
                    $"SMTP protocol error: {protocolEx.Message}", protocolEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error while sending email to {ToEmail}",
                    mailRequest.ToEmail);

                throw new InvalidOperationException(
                    $"Failed to send email: {ex.Message}", ex);
            }
            finally
            {
                // Disconnect asynchronously with quit
                if (client.IsConnected)
                {
                    try
                    {
                        await client.DisconnectAsync(quit: true);
                        _logger.LogDebug("SMTP client disconnected.");
                    }
                    catch (Exception disconnectEx)
                    {
                        _logger.LogWarning(disconnectEx,
                            "Error during SMTP client disconnect");
                    }
                }
            }
        }

        /// <summary>
        /// Builds a MimeMessage from the mail request.
        /// </summary>
        private MimeMessage BuildMimeMessage(MailRequest mailRequest)
        {
            var email = new MimeMessage();

            // Set sender
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));

            // Set recipient
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));

            // Set subject
            email.Subject = mailRequest.Subject;

            // Set body (HTML)
            var bodyBuilder = new BodyBuilder { HtmlBody = mailRequest.Body };

            // Attach any linked resources (e.g., inline images) if provided
            if (mailRequest.LinkedResources != null)
            {
                foreach (var resource in mailRequest.LinkedResources)
                {
                    bodyBuilder.LinkedResources.Add(resource);
                }
            }

            email.Body = bodyBuilder.ToMessageBody();

            return email;
        }
    }
}
