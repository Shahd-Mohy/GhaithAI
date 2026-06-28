using GhaithAI.API.DTOs.Auth;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.GaithAI.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GhaithAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginDTO dto)
        {
            var result = await _authService.GoogleLoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("register-clinician")]
        public async Task<IActionResult> RegisterClinician([FromForm] RegisterClinicianDTO dto)
        {
            var result = await _authService.RegisterClinicianAsync(dto);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new
            {
                message = "Password reset successfully."
            });
        }
    }
}