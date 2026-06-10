using GhaithAI.GaithAI.Application.DTOs.ClinicPatient;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicPatientController : ControllerBase
    {
        private readonly IClinicPatientService _patientService;

        public ClinicPatientController(IClinicPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("doctor/{doctorId:guid}")]
        public async Task<IActionResult> GetPatients(
            [FromRoute] Guid doctorId,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var patients = await _patientService.GetPatientsByClinicAsync(doctorId, searchTerm, pageNumber, pageSize);
            return Ok(patients);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClinicPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _patientService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateClinicPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _patientService.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var message = await _patientService.DeleteAsync(id);
            return Ok(new { message });
        }
    }
}
