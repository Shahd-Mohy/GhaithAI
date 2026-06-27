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
    [AllowAnonymous]
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

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
        {
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

            var mailRequest = new MailRequest
            {
                ToEmail = request.ToEmail,
                Subject = "GhaithAI Test Email",
                Body = request.Body
            };

            _logger.LogInformation("Sending test email to {ToEmail}", request.ToEmail);
            await _mailService.SendEmailAsync(mailRequest);

            _logger.LogInformation("Test email successfully sent to {ToEmail}", request.ToEmail);

            return Ok(new
            {
                message = "Email sent successfully",
                recipient = request.ToEmail,
                timestamp = DateTime.UtcNow
            });
        }
    }

    public sealed class SendTestEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
