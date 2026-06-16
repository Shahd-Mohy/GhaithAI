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
        private readonly IDoctorClinicProfileService _service;

        public DoctorClinicProfileController(IDoctorClinicProfileService service)
        {
            _service = service;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token.");


        // ─── DOCTOR SIDE ─────────────────────────────────────────────────────────

        // GET /api/doctor/clinic/profile
        [HttpGet("api/doctor/clinic/profile")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _service.GetMyProfileAsync(GetUserId());
            return Ok(result);
        }

        // PUT /api/doctor/clinic/profile
        [HttpPut("api/doctor/clinic/profile")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorClinicProfileDto dto)
        {
            var result = await _service.UpdateMyProfileAsync(GetUserId(), dto);
            return Ok(result);
        }

        // PATCH /api/doctor/clinic/profile/public-listing
        [HttpPatch("api/doctor/clinic/profile/public-listing")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> SetPublicListing([FromBody] SetPublicListingDto dto)
        {
            var result = await _service.SetPublicListingAsync(GetUserId(), dto.IsPublicListed);
            return Ok(result);
        }

        // GET /api/doctor/clinic/schedule/default
        [HttpGet("api/doctor/clinic/schedule/default")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> GetDefaultSchedule()
        {
            var result = await _service.GetDefaultScheduleAsync(GetUserId());
            return Ok(result);
        }

        // PUT /api/doctor/clinic/schedule/default
        [HttpPut("api/doctor/clinic/schedule/default")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> UpsertDefaultSchedule([FromBody] List<UpsertScheduleDto> slots)
        {
            var result = await _service.UpsertDefaultScheduleAsync(GetUserId(), slots);
            return Ok(result);
        }

        // GET /api/doctor/clinic/schedule/custom
        [HttpGet("api/doctor/clinic/schedule/custom")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> GetCustomSchedules(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var result = await _service.GetCustomSchedulesAsync(GetUserId(), from, to);
            return Ok(result);
        }

        // POST /api/doctor/clinic/schedule/custom
        [HttpPost("api/doctor/clinic/schedule/custom")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> AddCustomSchedule([FromBody] UpsertCustomScheduleDto dto)
        {
            var result = await _service.AddCustomScheduleAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetCustomSchedules), result);
        }

        // PUT /api/doctor/clinic/schedule/custom/{id}
        [HttpPut("api/doctor/clinic/schedule/custom/{id:guid}")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> UpdateCustomSchedule(Guid id, [FromBody] UpsertCustomScheduleDto dto)
        {
            var result = await _service.UpdateCustomScheduleAsync(GetUserId(), id, dto);
            return Ok(result);
        }

        // DELETE /api/doctor/clinic/schedule/custom/{id}
        [HttpDelete("api/doctor/clinic/schedule/custom/{id:guid}")]
        [Authorize(Roles = "Clinician")]
        public async Task<IActionResult> DeleteCustomSchedule(Guid id)
        {
            await _service.DeleteCustomScheduleAsync(GetUserId(), id);
            return NoContent();
        }


        // ─── PATIENT SIDE ─────────────────────────────────────────────────────────

        // GET /api/professionals
        [HttpGet("api/professionals")]
        [Authorize]
        public async Task<IActionResult> GetPublicDoctors(
            [FromQuery] string? search,
            [FromQuery] string? specialty,
            [FromQuery] string? language,
            [FromQuery] string? sessionType,
            [FromQuery] string? city,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetPublicDoctorsAsync(
                search, specialty, language, sessionType, city, page, pageSize);
            return Ok(result);
        }

        // GET /api/professionals/{doctorId}
        [HttpGet("api/professionals/{doctorId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetPublicDoctorProfile(Guid doctorId)
        {
            var result = await _service.GetPublicDoctorProfileAsync(doctorId);
            return Ok(result);
        }

        // GET /api/professionals/{doctorId}/available-slots
        [HttpGet("api/professionals/{doctorId:guid}/available-slots")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSlots(
            Guid doctorId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (from > to)
                return BadRequest("'from' must be before 'to'.");

            if ((to - from).TotalDays > 31)
                return BadRequest("Date range cannot exceed 31 days.");

            var result = await _service.GetAvailableSlotsAsync(doctorId, from, to);
            return Ok(result);
        }
    }
}
