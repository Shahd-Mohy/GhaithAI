
using GhaithAI.GaithAI.Application.DTOs.SelfHelp;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SelfHelpAdminController : ControllerBase
    {
        private readonly ISelfHelpAdminService _adminService;
        private readonly ILogger<SelfHelpAdminController> _logger;

        public SelfHelpAdminController(ISelfHelpAdminService adminService, ILogger<SelfHelpAdminController> logger)
        {
            _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [HttpGet("Get_All")]
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

        [HttpGet("Get_Content_Details/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
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

        [HttpPost("Create_SelfHelp_Content")]
        public async Task<IActionResult> Create([FromBody] AdminSelfHelpSaveDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Admin Controller: Invalid Model State submitted for creation.");
                return BadRequest(ModelState);
            }

            if (dto == null)
                return BadRequest(new { message = "Payload configuration error. Content body cannot be null." });

            string currentAdminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                    ?? GetCurrentAdminId();

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
        [HttpPut("Update_SelfHelp_Content")]
        public async Task<IActionResult> Update([FromBody] AdminSelfHelpUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string currentAdminId = GetCurrentAdminId();
            if (string.IsNullOrWhiteSpace(currentAdminId)) return Unauthorized();

            try
            {
                var result = await _adminService.UpdateContentAsync(dto, currentAdminId);
                return Ok(new { success = result, message = "Content and associated tips updated successfully." });
            }
            catch (ApplicationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        [HttpDelete("Delete_SelfHelp_Content/{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest(new { message = "Invalid ID." });

            string currentAdminId = GetCurrentAdminId();
            if (string.IsNullOrWhiteSpace(currentAdminId)) return Unauthorized();

            try
            {
                var result = await _adminService.DeleteContentAsync(id, currentAdminId);
                return Ok(new { success = result, message = "Content marked as deleted successfully." });
            }
            catch (ApplicationException ex)
            {
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
