using Azure.Core;
using GhaithAI.API.DTOs.Chat;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GhaithAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILangflowService _langflowService;
        public ChatController(ILangflowService langflowService)
        {
           _langflowService = langflowService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO requestDto)
        {
            if (string.IsNullOrWhiteSpace(requestDto.Message))
                return BadRequest("empty msg");
            var response = await _langflowService.SendMessageAsync(requestDto.Message, requestDto.SessionId);

            return Ok(new ChatResponseDTO
            {
                Reply =response,
                SessionId = requestDto.SessionId
            });
        }
    }
}
