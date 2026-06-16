using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.DoctorProfile;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorClinicProfileController : ControllerBase
    {
        private readonly IDoctorClinicProfileService _profileService;

        public DoctorClinicProfileController(IDoctorClinicProfileService profileService)
        {
            _profileService = profileService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");

        // DOCTOR SIDE
        [HttpGet("my-clinic")]

        [Authorize(Roles = Roles.Clinician)]
        public async Task<IActionResult> GetMyClinic()
        {
            try
            {
                var result = await _profileService.GetMyProfileAsync(GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT api/DoctorClinicProfile/my-clinic
        [HttpPut("my-clinic")]
        [Authorize(Roles = Roles.Clinician)]
        public async Task<IActionResult> UpdateMyClinic([FromBody] UpdateDoctorClinicProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _profileService.UpdateMyProfileAsync(GetUserId(), dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PATIENT SIDE

        // GET api/DoctorClinicProfile/professionals
        [HttpGet("professionals")]
        [Authorize]
        public async Task<IActionResult> GetPublicDoctors(
            [FromQuery] string? searchTerm,
            [FromQuery] string? specialty,
            [FromQuery] string? language,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _profileService.GetPublicDoctorsAsync(
                searchTerm, specialty, language, pageNumber, pageSize);
            return Ok(result);
        }

        // GET api/DoctorClinicProfile/professionals/{doctorId}

        [HttpGet("professionals/{doctorId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetPublicDoctorProfile([FromRoute] Guid doctorId)
        {
            try
            {
                var result = await _profileService.GetPublicDoctorProfileAsync(doctorId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(new { message = ex.Message }); 
            }
        }
    }
}
