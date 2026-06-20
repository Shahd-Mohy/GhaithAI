using System.Net;
using System.Net.Mail;

namespace GhaithAI.API.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(
            string toEmail,
            string resetLink)
        {
            var host = _configuration["MailSettings:Host"];
            var port = int.Parse(_configuration["MailSettings:Port"]!);
            var senderEmail = _configuration["MailSettings:Mail"];
            var senderName = _configuration["MailSettings:DisplayName"];
            var password = _configuration["MailSettings:Password"];

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = "Reset Your GhaithAI Password",
                IsBodyHtml = true,
                Body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: #0B8FAC; padding: 30px; text-align: center; border-radius: 12px 12px 0 0;'>
                        <h1 style='color: white; margin: 0; font-size: 24px;'>GhaithAI</h1>
                        <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0;'>
                            Mental Health Support Platform
                        </p>
                    </div>
                    <div style='background: white; padding: 40px; border-radius: 0 0 12px 12px; border: 1px solid #E2ECF0;'>
                        <h2 style='color: #0D1B3E; margin-bottom: 16px;'>Reset Your Password</h2>
                        <p style='color: #64748B; line-height: 1.6;'>
                            We received a request to reset your password.
                            Click the button below to create a new password.
                            This link will expire in <strong>1 hour</strong>.
                        </p>
                        <div style='text-align: center; margin: 32px 0;'>
                            <a href='{resetLink}'
                               style='background: #0B8FAC; color: white; padding: 14px 32px;
                                      border-radius: 9px; text-decoration: none; font-weight: 600;
                                      font-size: 15px; display: inline-block;'>
                                Reset Password
                            </a>
                        </div>
                        <p style='color: #94A3B8; font-size: 12px; line-height: 1.6;'>
                            If you didn't request this, you can safely ignore this email.
                            Your password will not be changed.
                        </p>
                        <hr style='border: none; border-top: 1px solid #E2ECF0; margin: 24px 0;'>
                        <p style='color: #94A3B8; font-size: 11px; text-align: center;'>
                            GhaithAI · HIPAA &amp; GDPR Compliant · Egypt PDPL Law 151
                        </p>
                    </div>
                </div>"
            };

            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}