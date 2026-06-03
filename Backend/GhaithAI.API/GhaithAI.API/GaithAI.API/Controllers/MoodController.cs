using GhaithAI.API.DTOs.Mood;
using GhaithAI.API.GaithAI.Application.DTOs.Mood;
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
    public class MoodController : ControllerBase
    {
        private readonly IMoodService _moodService;

        public MoodController(IMoodService moodService)
        {
            _moodService = moodService;
        }


        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");

        // POST /api/mood/log

        [HttpPost("log")]
        public async Task<IActionResult> LogMood([FromBody] CreateMoodLogDTO dto)
        {
            var userId = GetUserId();
            var result = await _moodService.LogMoodAsync(userId, dto);

            return StatusCode(201, SuccessResponse<MoodLogCreatedDTO>.Created(result, "Mood logged successfully."));
        }

        // GET /api/mood/calendar?year=2026&month=5

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
        {
            if (year < 2020 || year > 2100 || month < 1 || month > 12)
                return BadRequest(ErrorResponse.BadRequest("Invalid year or month."));

            var userId = GetUserId();
            var result = await _moodService.GetCalendarAsync(userId, year, month);

            return Ok(SuccessResponse<List<CalendarDayDTO>>.Ok(result));
        }

        // GET /api/mood/statistics?period=month

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string period = "month")
        {
            if (period != "week" && period != "month")
                return BadRequest(ErrorResponse.BadRequest("Period must be 'week' or 'month'."));

            var userId = GetUserId();
            var result = await _moodService.GetStatisticsAsync(userId, period);

            return Ok(SuccessResponse<MoodStatisticsDTO>.Ok(result));
        }

        // GET /api/mood/history?page=1&pageSize=20 //Han4offffff

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = GetUserId();
            var (items, total) = await _moodService.GetHistoryAsync(userId, from, to, page, pageSize);

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

        // GET /api/mood/{moodLogId}

        [HttpGet("{moodLogId:guid}")]
        public async Task<IActionResult> GetById(Guid moodLogId)
        {
            var userId = GetUserId();
            var result = await _moodService.GetByIdAsync(userId, moodLogId);

            if (result == null)
                return NotFound(ApiResponse.NotFound("Mood log not found."));

            return Ok(SuccessResponse<MoodHistoryDTO>.Ok(result));
        }

        // PUT /api/mood/{moodLogId}

        [HttpPut("{moodLogId:guid}")]
        public async Task<IActionResult> Update(Guid moodLogId, [FromBody] UpdateMoodLogDTO dto)
        {
            var userId = GetUserId();
            var success = await _moodService.UpdateAsync(userId, moodLogId, dto);

            if (!success)
                return NotFound(ApiResponse.NotFound("Mood log not found or does not belong to you."));

            return Ok(ApiResponse.Ok("Mood log updated successfully."));
        }

        // GET /api/mood/export //Save for later 

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var userId = GetUserId();
            var csvBytes = await _moodService.ExportCsvAsync(userId);

            return File(csvBytes, "text/csv", $"mood_export_{DateTime.UtcNow:yyyy-MM-dd}.csv");
        }
    }
}
