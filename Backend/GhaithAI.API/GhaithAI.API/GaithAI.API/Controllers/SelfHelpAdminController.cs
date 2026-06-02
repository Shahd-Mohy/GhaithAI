
namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfHelpAdminController : ControllerBase
    {
        private readonly ISelfHelpAdminService _adminService;

        public SelfHelpAdminController(ISelfHelpAdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _adminService.GetAllContentAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminService.GetContentByIdAsync(id);
            if (result == null) return NotFound(new { message = "no content with this id " });
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminSelfHelpSaveDto dto)
        {
            string currentAdminId = "soltan";
            var result = await _adminService.CreateContentAsync(dto, currentAdminId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminSelfHelpSaveDto dto)
        {
            string currentAdminId = "Soltan";
            var result = await _adminService.UpdateContentAsync(id, dto, currentAdminId);
            if (result == null) return NotFound(new { message = "no content with this id for edit " });
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            string currentAdminId = "Soltan";
            var isDeleted = await _adminService.SoftDeleteContentAsync(id, currentAdminId);
            if (!isDeleted) return NotFound(new { message = "no content for delete " });
            return Ok(new { message = "succsffuly deleted" });
        }
    }
}
