
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfHelpAdminController : ControllerBase
    {
        private readonly ISelfHelpAdminService _adminService;
        private readonly ILogger<SelfHelpAdminController> _logger;

        public SelfHelpAdminController(ISelfHelpAdminService adminService, ILogger<SelfHelpAdminController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _adminService.GetAllContentAsync();
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Admin Controller: Failed to fetch all self-help contents.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Admin Controller: GetById requested with an empty Guid.");
                return BadRequest(new { message = "The provided identifier (ID) cannot be empty." });
            }
            try
            {
                var result = await _adminService.GetContentByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"No self-help content found with ID: {id}" });

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Admin Controller: Error fetching content by ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminSelfHelpSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin Controller: Invalid Model State submitted for creation.");
                return BadRequest(ModelState); 
            }

            if (dto == null)
                return BadRequest(new { message = "Payload configuration error. Content body cannot be null." });

            string currentAdminId = GetCurrentAdminId();

            if (string.IsNullOrWhiteSpace(currentAdminId))
            {
                _logger.LogCritical("Security Breach: Unauthorized or unidentified entity tried to create content.");
                return Unauthorized(new { message = "Action denied. Admin identity missing from context." });
            }

            try
            {
                var result = await _adminService.CreateContentAsync(dto, currentAdminId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Admin Controller: Persistence layer failure during creation by Admin: {AdminId}", currentAdminId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminSelfHelpSaveDto dto)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Invalid resource identifier. Target ID cannot be empty." });

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin Controller: Invalid Model State submitted for updating ID: {Id}", id);
                return BadRequest(ModelState);
            }

            string currentAdminId = GetCurrentAdminId();

            if (string.IsNullOrWhiteSpace(currentAdminId))
                return Unauthorized(new { message = "Action denied. Valid Admin credentials required." });

            try
            {
                var result = await _adminService.UpdateContentAsync(id, dto, currentAdminId);
                if (result == null)
                    return NotFound(new { message = "No active content matches this ID, or the record has been locked/deleted." });

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Admin Controller: Exception thrown during updating content ID: {Id} by Admin: {AdminId}", id, currentAdminId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new { message = "Invalid resource identifier for deletion." });

            string currentAdminId = GetCurrentAdminId();
            if (string.IsNullOrWhiteSpace(currentAdminId))
                return Unauthorized(new { message = "Action denied. Valid Admin credentials required." });

            try
            {
                var isDeleted = await _adminService.SoftDeleteContentAsync(id, currentAdminId);
                if (!isDeleted)
                    return NotFound(new { message = "The target content does not exist or has already been soft-deleted." });

                return Ok(new { message = "The content has been successfully soft-deleted from active records." });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Admin Controller: Soft delete transaction failed for content ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }
        private string GetCurrentAdminId()
        {
            if (User == null) return string.Empty;

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("uid")?.Value
                   ?? string.Empty;
        }
    }
}
