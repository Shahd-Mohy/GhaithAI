using GhaithAI.GaithAI.Application.DTOs.SessionNote;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;
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
            var noteId = await _noteService.AddNoteAsync(sessionId, dto);
            return StatusCode(201, new { id = noteId, message = "Note added successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes([FromRoute] Guid sessionId)
        {
            var notes = await _noteService.GetNotesBySessionAsync(sessionId);
            return Ok(notes);
        }

        [HttpGet("/api/sessions/notes/mine")]
        public async Task<IActionResult> GetMyNotes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid user." });

            var notes = await _noteService.GetNotesByDoctorAsync(userId);
            return Ok(notes);
        }

        [HttpPut("{noteId:guid}")]
        public async Task<IActionResult> UpdateNote(
            [FromRoute] Guid sessionId,
            [FromRoute] Guid noteId,
            [FromBody] UpdateNoteDto dto)
        {
            await _noteService.UpdateNoteAsync(sessionId, noteId, dto);
            return Ok(new { message = "Note updated successfully." });
        }
    }
}
