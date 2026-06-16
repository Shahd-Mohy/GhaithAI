using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.Specialty;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseSpecialtyController : ControllerBase
    {
        private readonly IBaseSpecialtyService _specialtyService;
        public BaseSpecialtyController(IBaseSpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }
        [HttpGet("getAll/dropDown")]
        public async Task<IActionResult> GetAll()
        {
            var specialties = await _specialtyService.GetAllAsync();
            return Ok(specialties);
        }
        [HttpGet("GetById/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            try
            {
                var specialty = await _specialtyService.GetByIdAsync(id);
                return Ok(specialty);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]

        [HttpPost("Create_Specialty")]
        public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _specialtyService.CreateAsync(dto);
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

        [HttpPut("Update_Specialty")]
        public async Task<IActionResult> Update([FromBody] UpdateSpecialtyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _specialtyService.UpdateAsync(dto);
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

        [HttpDelete("Delete_Specialty/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var message = await _specialtyService.DeleteAsync(id);
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
        }  }
}
