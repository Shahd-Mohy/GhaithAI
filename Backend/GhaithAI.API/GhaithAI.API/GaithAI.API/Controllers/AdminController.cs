using GhaithAI.API.Constants;
using GhaithAI.GaithAI.Application.DTOs.Admin;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.GaithAI.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
     [Authorize(Roles = Roles.Admin)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ─── GET Lists ────────────────────────────────────
        [HttpGet("doctors/pending")]
        public async Task<IActionResult> GetPending()
        {
            try
            {
                var result = await _adminService.GetPendingDoctorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctors/approved")]
        public async Task<IActionResult> GetApproved()
        {
            try
            {
                var result = await _adminService.GetApprovedDoctorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("doctors/rejected")]
        public async Task<IActionResult> GetRejected()
        {
            try
            {
                var result = await _adminService.GetRejectedDoctorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── GET Details ──────────────────────────────────
        [HttpGet("doctors/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var result = await _adminService.GetDoctorDetailsAsync(id);
                if (result == null)
                    return NotFound(new { message = "Doctor not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Approve ──────────────────────────────────────
        [HttpPost("doctors/{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _adminService.ApproveDoctorAsync(id);
                return Ok(new { message = "Doctor approved successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Reject ───────────────────────────────────────
        [HttpPost("doctors/{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDoctorDTO dto)
        {
            try
            {
                await _adminService.RejectDoctorAsync(id, dto.Reason);
                return Ok(new { message = "Doctor rejected successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}