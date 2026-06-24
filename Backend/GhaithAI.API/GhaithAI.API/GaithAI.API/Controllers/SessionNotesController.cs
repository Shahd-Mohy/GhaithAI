using GhaithAI.GaithAI.Application.DTOs.SessionNote;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/sessions/{sessionId:guid}/notes")]
    [ApiController]
    [Authorize]
    public class SessionNotesController : ControllerBase
    {
        private readonly ISessionNoteService _noteService;

        public SessionNotesController(ISessionNoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(
            [FromRoute] Guid sessionId,
            [FromBody] AddNoteDto dto)
        {
            try
            {
                var noteId = await _noteService.AddNoteAsync(sessionId, dto);
                return StatusCode(201, new { id = noteId, message = "Note added successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes([FromRoute] Guid sessionId)
        {
            try
            {
                var notes = await _noteService.GetNotesBySessionAsync(sessionId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("/api/sessions/notes/mine")]
        public async Task<IActionResult> GetMyNotes()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid user." });

                var notes = await _noteService.GetNotesByDoctorAsync(userId);
                return Ok(notes);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{noteId:guid}")]
        public async Task<IActionResult> UpdateNote(
            [FromRoute] Guid sessionId,
            [FromRoute] Guid noteId,
            [FromBody] UpdateNoteDto dto)
        {
            try
            {
                await _noteService.UpdateNoteAsync(sessionId, noteId, dto);
                return Ok(new { message = "Note updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
