global using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfHelpController : ControllerBase
    {
        private readonly ISelfHelpUserService _userService;
        private readonly ILogger<SelfHelpController> _logger;

        public SelfHelpController(ISelfHelpUserService userService, ILogger<SelfHelpController> logger)
        {
            _userService = userService ??throw new ArgumentNullException(nameof(userService));
            _logger=logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActive([FromQuery] string? type, [FromQuery] string? difficulty)
        {
            string? cleanType = string.IsNullOrWhiteSpace(type) ? null : type.Trim();
            string? cleanDifficulty = string.IsNullOrWhiteSpace(difficulty) ? null : difficulty.Trim();

            if ((cleanType != null && cleanType.Length > 50) || (cleanDifficulty != null && cleanDifficulty.Length > 50))
            {
                _logger.LogWarning("GetAllActive called with suspiciously long query parameters. Type length: {TypeLen}, Difficulty length: {DiffLen}",
                    cleanType?.Length ?? 0, cleanDifficulty?.Length ?? 0);
                return BadRequest(new { message = "Query parameters exceed allowed length limits." });
            }

            var result = await _userService.GetActiveContentAsync(cleanType, cleanDifficulty);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetById requested with an empty Guid.");
                return BadRequest(new { message = "The provided ID is invalid or empty." });
            }
            try
            {
                var result = await _userService.GetContentByIdAsync(id);
                if (result == null)
                {
                    _logger.LogInformation("Content with ID: {ContentId} was not found or is inactive.", id);
                    return NotFound(new { message = "The requested self-help content is not available." });
                }
                return Ok(result);

            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application exception caught in Controller for ID: {ContentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
    }
}
