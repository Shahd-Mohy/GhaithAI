using GhaithAI.API.DTOs.Journal;
using GhaithAI.API.GaithAI.Application.DTOs.Journal;
using GhaithAI.API.Responses;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JournalController : ControllerBase
    {
        private readonly IJournalService _journalService;

        public JournalController(IJournalService journalService)
        {
            _journalService = journalService;
        }

        private string GetUserId() =>
          User.FindFirstValue(ClaimTypes.NameIdentifier)
          ?? throw new UnauthorizedAccessException("User ID not found in token");

        // GET /api/journal

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = GetUserId();
            var (items, total) = await _journalService.GetAllAsync(userId, search, page, pageSize);

            return Ok(new
            {
                Success = true,
                Data = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }

        // GET /api/journal/{id}

        [HttpGet("{journalId:guid}")]
        public async Task<IActionResult> GetById(Guid journalId)
        {
            var userId = GetUserId();
            var result = await _journalService.GetByIdAsync(userId, journalId);

            if (result == null)
                return NotFound(ApiResponse.NotFound("Journal entry not found."));

            return Ok(SuccessResponse<JournalDTO>.Ok(result));
        }

        // POST /api/journal

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJournalDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(ErrorResponse.BadRequest("Content is required."));

            var userId = GetUserId();
            var result = await _journalService.CreateAsync(userId, dto);

            return StatusCode(201, SuccessResponse<JournalCreatedDTO>.Created(result, "Journal entry created successfully."));
        }

        // PUT /api/journal/{id}

        [HttpPut("{journalId:guid}")]
        public async Task<IActionResult> Update(Guid journalId, [FromBody] UpdateJournalDTO dto)
        {
            var userId = GetUserId();
            var success = await _journalService.UpdateAsync(userId, journalId, dto);

            if (!success)
                return NotFound(ApiResponse.NotFound("Journal entry not found or does not belong to you."));

            return Ok(ApiResponse.Ok("Journal entry updated successfully."));
        }

        // DELETE /api/journal/{id}

        [HttpDelete("{journalId:guid}")]
        public async Task<IActionResult> Delete(Guid journalId)
        {
            var userId = GetUserId();
            var success = await _journalService.DeleteAsync(userId, journalId);

            if (!success)
                return NotFound(ApiResponse.NotFound("Journal entry not found or does not belong to you."));

            return Ok(ApiResponse.Ok("Journal entry deleted successfully."));
        }
    }
}
