using GhaithAI.API.DTOs.Auth;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.GaithAI.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class AuthController
        : ControllerBase
    {
        private readonly IAuthService
            _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            try
            {
                var result =
                await _authService.RegisterAsync(dto);

            return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }

        }


        [HttpPost("login")]
        public async Task<IActionResult>
            Login(LoginDTO dto)
        {
            var result =
                await _authService
                    .LoginAsync(dto);

            return Ok(result);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult>
            GoogleLogin(
            GoogleLoginDTO dto)
        {
            var result =
                await _authService
                .GoogleLoginAsync(dto);

            return Ok(result);
        }
    }
}