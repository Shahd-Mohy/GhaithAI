//using GhaithAI.API.DTOs.Chat;
//using GhaithAI.API.Services.Interfaces;

//using GhaithAI.API.GaithAI.Application.DTOs.Chat;
//using Microsoft.AspNetCore.Mvc;

//namespace GhaithAI.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChatController : ControllerBase
//    {
//        private readonly ILangflowService _langflowService;
//        public ChatController(ILangflowService langflowService)
//        {
//           _langflowService = langflowService;
//        }
//        [HttpPost("send")]
//        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO requestDto)
//        {
//            if (string.IsNullOrWhiteSpace(requestDto.Message))
//                return BadRequest("empty msg");
//            var response = await _langflowService.SendMessageAsync(requestDto.Message, requestDto.SessionId);

//            return Ok(new ChatResponseDTO
//            {
//                Reply =response,
//                SessionId = requestDto.SessionId
//            });
//            _langflowService = langflowService;
//        }

//        [HttpPost("send")]
//        public async Task<ActionResult<GhaithFinalResultDto>> SendMessage([FromBody] UserChatRequestDto  requestDto)
//        {
//            if (requestDto == null || string.IsNullOrWhiteSpace(requestDto.Message))
//                return BadRequest("empty msg");

//            if (!Guid.TryParse(requestDto.SessionId, out Guid parsedSessionId))
//                return BadRequest("’Ì€… «·‹ SessionId €Ì— ’ÕÌÕ…° ÌÃ» √‰  ﬂÊ‰ Guid.");

//            try
//            {
//                var aiResult = await _langflowService.ProcessUserMessageAsync(parsedSessionId, requestDto.Message);
//                return Ok(aiResult);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }



//        }
//    }
//}

using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.API.GaithAI.Application.DTOs.Chat;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILangflowService _langflowService;

        public ChatController(
            ILangflowService langflowService)
        {
            _langflowService = langflowService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(
            [FromBody] ChatRequestDTO requestDto)
        {
            if (string.IsNullOrWhiteSpace(requestDto.Message))
                return BadRequest("empty msg");

            var response =
                await _langflowService.SendMessageAsync(
                    requestDto.Message,
                    requestDto.SessionId);

            return Ok(new ChatResponseDTO
            {
                Reply = response,
                SessionId = requestDto.SessionId
            });
        }

        [HttpPost("send-v2")]
        public async Task<ActionResult<GhaithFinalResultDto>>
            SendMessageV2(
            [FromBody] UserChatRequestDto requestDto)
        {
            if (requestDto == null ||
                string.IsNullOrWhiteSpace(requestDto.Message))
            {
                return BadRequest("empty msg");
            }

            if (!Guid.TryParse(
                requestDto.SessionId,
                out Guid parsedSessionId))
            {
                return BadRequest("Invalid SessionId");
            }

            try
            {
                var aiResult =
                    await _langflowService
                        .ProcessUserMessageAsync(
                            parsedSessionId,
                            requestDto.Message);

                return Ok(aiResult);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    $"Internal server error: {ex.Message}");
            }
        }
    }
}