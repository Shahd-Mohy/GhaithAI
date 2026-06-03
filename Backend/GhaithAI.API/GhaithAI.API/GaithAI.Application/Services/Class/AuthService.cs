using GhaithAI.API.Constants;
using GhaithAI.API.Data;
using GhaithAI.API.DTOs.Auth;
using GhaithAI.API.Helpers;
using GhaithAI.API.Models;
using GhaithAI.API.Presistance;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.GaithAI.Application.DTOs.Auth;
using GhaithAI.GaithAI.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;

namespace GhaithAI.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<AuthResponseDTO> RegisterAsync(GhaithAI.API.DTOs.Auth.RegisterDTO dto)
        {
            var exists =
                await _userManager.FindByEmailAsync(dto.Email);

            if (exists != null)
            {
                throw new Exception("Email already exists");
            }

            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CountryCode = dto.CountryCode,
                PreferredLanguage = dto.PreferredLanguage,

                AcceptedTerms = dto.AcceptedTerms,
                AcceptedPrivacyPolicy = dto.AcceptedPrivacyPolicy,
                AcceptedAiChat = dto.AcceptedAiChat,
                AcceptedMoodTracking = dto.AcceptedMoodTracking,
                AcceptedDataCollection = dto.AcceptedDataCollection,

                ConsentedAt = DateTime.UtcNow
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    dto.Password);

            if (!result.Succeeded)
            {
                throw new Exception(
                    result.Errors.First().Description);
            }

            await _userManager.AddToRoleAsync(
                user,
                Roles.User);

            var contacts = new List<EmergencyContact>
            {
                new EmergencyContact
                {
                    UserId = user.Id,
                    FullName = dto.FirstContact.FullName,
                    Relationship = dto.FirstContact.Relationship,
                    PhoneNumber = dto.FirstContact.PhoneNumber,
                    PriorityOrder = 1
                },

                new EmergencyContact
                {
                    UserId = user.Id,
                    FullName = dto.SecondContact.FullName,
                    Relationship = dto.SecondContact.Relationship,
                    PhoneNumber = dto.SecondContact.PhoneNumber,
                    PriorityOrder = 2
                }
            };


            await _context.EmergencyContacts.AddRangeAsync(contacts);
            await _context.SaveChangesAsync();

            var profile = new UserAssessment
            {
                UserId = user.Id,

                Age = dto.Age,

                Concerns = string.Join(",", dto.Concerns),

                SleepQuality = dto.SleepQuality,

                StressLevel = dto.StressLevel,

                HasTherapyHistory = dto.HasTherapyHistory,

                TakesMedication = dto.TakesMedication
            };

            await _context.UserAssessments.AddAsync(profile);

            await _context.SaveChangesAsync();

            var roles =
                await _userManager.GetRolesAsync(user);

            var token =
                JWTTokenHelper.GenerateToken(
                    user,
                    _configuration,
                    roles);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ProfilePicture = user.ProfilePicture,
                Expiration = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user =
                await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                throw new Exception("Invalid Email");

            var valid =
                await _userManager.CheckPasswordAsync(
                    user,
                    dto.Password);

            if (!valid)
                throw new Exception("Invalid Password");

            user.LastLoginAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            var roles =
                await _userManager.GetRolesAsync(user);

            var token =
                JWTTokenHelper.GenerateToken(
                    user,
                    _configuration,
                    roles);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ProfilePicture = user.ProfilePicture,
                Expiration = DateTime.UtcNow.AddDays(7)
            };
        }

        public async Task<AuthResponseDTO> GoogleLoginAsync(
            GoogleLoginDTO dto)
        {
            var payload =
                await GoogleJsonWebSignature
                    .ValidateAsync(dto.IdToken);

            var user =
                await _userManager
                    .FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    FullName = payload.Name,
                    GoogleId = payload.Subject,
                    IsGoogleAccount = true,
                    EmailConfirmed = true,
                    ProfilePicture = payload.Picture,
                    CountryCode = "EG",
                    PreferredLanguage = "en",
                    CreatedAt = DateTime.UtcNow
                };

                var result =
                    await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        result.Errors.First().Description);
                }

                await _userManager.AddToRoleAsync(
                    user,
                    Roles.User);
            }

            var roles =
                await _userManager.GetRolesAsync(user);

            var token =
                JWTTokenHelper.GenerateToken(
                    user,
                    _configuration,
                    roles);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ProfilePicture = user.ProfilePicture,
                Expiration = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}