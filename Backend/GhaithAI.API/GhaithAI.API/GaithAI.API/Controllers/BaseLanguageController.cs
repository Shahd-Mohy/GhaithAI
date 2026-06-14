using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.Language;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseLanguageController : ControllerBase
    {
        private readonly IBaseLanguageService _languageService;

        public BaseLanguageController(IBaseLanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpGet("Dropdown")]
        public async Task<IActionResult> GetAll()
        {
            var languages = await _languageService.GetAllAsync();
            return Ok(languages);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            try
            {
                var language = await _languageService.GetByIdAsync(id);
                return Ok(language);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles =  Roles.Admin )]
        [HttpPost("Create_Language")]
        public async Task<IActionResult> Create([FromBody] CreateLanguageDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _languageService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("Update_Language")]
        public async Task<IActionResult> Update([FromBody] UpdateLanguageDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _languageService.UpdateAsync(dto);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("Delete_Language/{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            try
            {
                var message = await _languageService.DeleteAsync(id);
                return Ok(new { message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
