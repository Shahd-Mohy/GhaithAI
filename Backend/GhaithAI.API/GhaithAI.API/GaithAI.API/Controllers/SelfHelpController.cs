global using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfHelpController : ControllerBase
    {
        private readonly ISelfHelpUserService _userService;

        public SelfHelpController(ISelfHelpUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActive([FromQuery] string? type, [FromQuery] string? difficulty)
        {
            var result = await _userService.GetActiveContentAsync(type, difficulty);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _userService.GetContentByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "no content with this id " });

            return Ok(result);
        }
    }
}
