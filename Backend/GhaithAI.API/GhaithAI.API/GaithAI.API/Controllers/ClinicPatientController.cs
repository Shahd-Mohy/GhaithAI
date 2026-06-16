using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Clinician)]

    public class ClinicPatientController : ControllerBase
    {
        private readonly IClinicPatientService _patientService;

        public ClinicPatientController(IClinicPatientService patientService)
        {
            _patientService = patientService;
        }
        [HttpGet("My_Patients")]
        public async Task<IActionResult> GetPatients(
                    [FromQuery] string? searchTerm,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var patients = await _patientService.GetPatientsByClinicAsync(userId, searchTerm, pageNumber, pageSize);
            return Ok(patients);
        }

        [HttpGet("DetailPatient/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var patient = await _patientService.GetByIdAsync(id, userId);
            return Ok(patient);
        }

        [HttpPost("Create_Patient")]
        public async Task<IActionResult> Create([FromBody] CreateClinicPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var result = await _patientService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("Update_Patient")]
        public async Task<IActionResult> Update([FromBody] UpdateClinicPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            await _patientService.UpdateAsync(dto, userId);
            return NoContent();
        }

        [HttpDelete("Delete_Patients/{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid or missing identifiers in token." });

            var message = await _patientService.DeleteAsync(id, userId);
            return Ok(new { message });
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
