using GhaithAI.API.DTOs.User;
using GhaithAI.API.Interfaces.InterfaceService;
using GhaithAI.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GhaithAI.API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]

    [Authorize]
    public class UserController
        : ControllerBase
    {
        private readonly IUserService
            _userService;

        public UserController(
            IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult>
            GetProfile()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var profile =
                await _userService
                    .GetProfileAsync(userId);

            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult>
            UpdateProfile(
                UpdateProfileDTO dto)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var updated =
                await _userService
                    .UpdateProfileAsync(
                        userId,
                        dto);

            return Ok(updated);
        }
    }
}