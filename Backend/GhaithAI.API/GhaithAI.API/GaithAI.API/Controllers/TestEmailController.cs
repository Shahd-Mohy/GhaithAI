using GhaithAI.API.GaithAI.Application.DTOs.Mail;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.Controllers
{
    /// <summary>
    /// Controller for testing email functionality.
    /// WARNING: This endpoint is unsecured and intended for rapid testing only.
    /// Remove or secure this controller in production.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // For rapid testing only — REMOVE IN PRODUCTION
    public sealed class TestEmailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(
            IMailService mailService,
            ILogger<TestEmailController> logger)
        {
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends a test email to the specified recipient.
        /// </summary>
        /// <param name="request">The test email request containing recipient and HTML body.</param>
        /// <returns>
        /// 200 OK if email sent successfully.
        /// 400 Bad Request if validation fails.
        /// 500 Internal Server Error if SMTP send fails.
        /// </returns>
        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
        {
            // ── 1. Validate request
            if (request is null)
            {
                _logger.LogWarning("Test email request is null");
                return BadRequest(new { error = "Request body cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                _logger.LogWarning("Test email request missing ToEmail");
                return BadRequest(new { error = "ToEmail field is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Body))
            {
                _logger.LogWarning("Test email request missing Body");
                return BadRequest(new { error = "Body field is required." });
            }

            // ── 2. Build mail request
            var mailRequest = new MailRequest
            {
                ToEmail = request.ToEmail,
                Subject = "GhaithAI Test Email",
                Body = request.Body
            };

            // ── 3. Attempt send
            try
            {
                _logger.LogInformation(
                    "Sending test email to {ToEmail}",
                    request.ToEmail);

                await _mailService.SendEmailAsync(mailRequest);

                _logger.LogInformation(
                    "Test email successfully sent to {ToEmail}",
                    request.ToEmail);

                return Ok(new
                {
                    message = "Email sent successfully",
                    recipient = request.ToEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx,
                    "Validation error in test email send: {Message}",
                    argEx.Message);

                return BadRequest(new { error = argEx.Message });
            }
            catch (InvalidOperationException invalidOpEx)
            {
                _logger.LogError(invalidOpEx,
                    "SMTP configuration or send failure in test email: {Message}",
                    invalidOpEx.Message);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "Email service error: " + invalidOpEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error in test email send to {ToEmail}: {Message}",
                    request.ToEmail, ex.Message);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred while sending the email." });
            }
        }
    }

    /// <summary>
    /// Request model for test email endpoint.
    /// </summary>
    public sealed class SendTestEmailRequest
    {
        /// <summary>Recipient email address (e.g., "test@example.com").</summary>
        public string ToEmail { get; set; } = string.Empty;

        /// <summary>Email body content (HTML supported).</summary>
        public string Body { get; set; } = string.Empty;
    }
}
