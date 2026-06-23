using GhaithAI.GaithAI.Application.DTOs.Transcript;
using GhaithAI.GaithAI.Domain.Interfaces.InterfaceService;
using Microsoft.AspNetCore.Authorization;

namespace GhaithAI.GaithAI.API.Controllers
{
    [Route("api/sessions/{sessionId:guid}/transcript")]
    [ApiController]
    [Authorize]
    public class SessionTranscriptsController : ControllerBase
    {
        private readonly ISessionTranscriptService _transcriptService;

        public SessionTranscriptsController(ISessionTranscriptService transcriptService)
        {
            _transcriptService = transcriptService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio(
            [FromRoute] Guid sessionId,
            [FromForm] UploadAudioDto dto)
        {
            if (dto.AudioFile == null || dto.AudioFile.Length == 0)
                return BadRequest(new { message = "Audio file is required." });

            try
            {
                await _transcriptService.UploadAndTriggerAsync(sessionId, dto.AudioFile);
                return StatusCode(202, new { message = "Audio received. Transcription is processing in the background." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus([FromRoute] Guid sessionId)
        {
            try
            {
                var status = await _transcriptService.GetStatusAsync(sessionId);
                return Ok(status);
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
        public async Task<IActionResult> GetTranscript([FromRoute] Guid sessionId)
        {
            try
            {
                var segments = await _transcriptService.GetSegmentsAsync(sessionId);
                return Ok(segments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{segmentId:guid}")]
        public async Task<IActionResult> UpdateSegment(
            [FromRoute] Guid sessionId,
            [FromRoute] Guid segmentId,
            [FromBody] UpdateSegmentDto dto)
        {
            try
            {
                await _transcriptService.UpdateSegmentAsync(sessionId, segmentId, dto);
                return Ok(new { message = "Segment updated successfully." });
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
