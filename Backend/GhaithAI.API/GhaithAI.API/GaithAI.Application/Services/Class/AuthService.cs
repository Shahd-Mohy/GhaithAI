using GhaithAI.API.Constants;
using GhaithAI.API.Data;
using GhaithAI.API.DTOs.Auth;
using GhaithAI.API.Helpers;
using GhaithAI.API.Models;
using GhaithAI.API.Services.Interfaces;
using GhaithAI.GaithAI.Application.DTOs.Auth;
using GhaithAI.GaithAI.Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
            _env = env;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);

            if (exists != null)
                throw new Exception("Email already exists");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    FullName = dto.FullName,
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    CountryCode = dto.CountryCode,
                    PreferredLanguage = dto.PreferredLanguage,

                    Gender = dto.Gender,

                    AcceptedTerms = dto.AcceptedTerms,
                    AcceptedPrivacyPolicy = dto.AcceptedPrivacyPolicy,
                    AcceptedAiChat = dto.AcceptedAiChat,
                    AcceptedMoodTracking = dto.AcceptedMoodTracking,
                    AcceptedDataCollection = dto.AcceptedDataCollection,

                    ConsentedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(
                    user,
                    dto.Password);

                if (!result.Succeeded)
                    throw new Exception(
                        result.Errors.First().Description);

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        Roles.User);

                if (!roleResult.Succeeded)
                    throw new Exception(
                        roleResult.Errors.First().Description);

                var contacts = new List<EmergencyContact>
        {
            new EmergencyContact
            {
                UserId = user.Id,
                FullName = dto.FirstContact.FullName,
                Relationship = dto.FirstContact.Relationship,
                PhoneNumber = dto.FirstContact.PhoneNumber,
                PriorityOrder = 1
            }
        };

                if (dto.SecondContact != null &&
                    !string.IsNullOrWhiteSpace(dto.SecondContact.FullName) &&
                    !string.IsNullOrWhiteSpace(dto.SecondContact.PhoneNumber))
                {
                    contacts.Add(new EmergencyContact
                    {
                        UserId = user.Id,
                        FullName = dto.SecondContact.FullName,
                        Relationship = dto.SecondContact.Relationship,
                        PhoneNumber = dto.SecondContact.PhoneNumber,
                        PriorityOrder = 2
                    });
                }

                await _context.EmergencyContacts.AddRangeAsync(contacts);

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

                await transaction.CommitAsync();

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
                    Expiration = DateTime.UtcNow.AddDays(7),
                    Role = roles.FirstOrDefault() ?? "User"
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                var createdUser =
                    await _userManager.FindByEmailAsync(dto.Email);

                if (createdUser != null)
                {
                    await _userManager.DeleteAsync(createdUser);
                }

                throw;
            }
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _context.Users
                .Include(x => x.DoctorsProfile)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid Email or Password");

            if (user.DoctorsProfile != null)
            {
                if (user.DoctorsProfile.ApprovalStatus ==
                    ApprovalStatus.Pending)
                {
                    throw new Exception(
                        "Your account is pending approval.");
                }

                if (user.DoctorsProfile.ApprovalStatus ==
                    ApprovalStatus.Rejected)
                {
                    throw new Exception(
                        $"Your account was rejected. Reason: {user.DoctorsProfile.RejectionReason}");
                }
            }

            if (user.IsGoogleAccount &&
                string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new Exception(
                    "This account uses Google Sign-In. Please login with Google.");
            }

            var valid =
                await _userManager.CheckPasswordAsync(
                    user,
                    dto.Password);

            if (!valid)
                throw new Exception("Invalid Email or Password");

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
                Expiration = DateTime.UtcNow.AddDays(7),
                Role = roles.FirstOrDefault() ?? "User"
            };
        }

        public async Task<AuthResponseDTO> GoogleLoginAsync(GoogleLoginDTO dto)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
            }
            catch
            {
                throw new Exception("Invalid Google token. Please try again.");
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);

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
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                    throw new Exception(result.Errors.First().Description);

                await _userManager.AddToRoleAsync(user, Roles.User);
            }
            else
            {
                user.GoogleId = payload.Subject;
                user.IsGoogleAccount = true;
                user.LastLoginAt = DateTime.UtcNow;

                if (string.IsNullOrEmpty(user.ProfilePicture))
                    user.ProfilePicture = payload.Picture;

                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = JWTTokenHelper.GenerateToken(user, _configuration, roles);

            return new AuthResponseDTO
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ProfilePicture = user.ProfilePicture,
                Expiration = DateTime.UtcNow.AddDays(7),
                Role = roles.FirstOrDefault() ?? "User"
            };
        }

        public async Task<AuthResponseDTO> RegisterClinicianAsync(
    RegisterClinicianDTO dto)
        {
            var exists =
                await _userManager.FindByEmailAsync(dto.Email);

            if (exists != null)
                throw new Exception("Email already exists");

            string pdfPath = "";

            if (dto.DocumentsPdf != null)
            {
                var fileName =
                    FileUploadHelper.GenerateFileName(
                        dto.DocumentsPdf.FileName);

                var uploadFolder =
                    Path.Combine(
                        _env.WebRootPath,
                        "uploads",
                        "doctors");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create);

                await dto.DocumentsPdf.CopyToAsync(stream);

                pdfPath =
                    $"/uploads/doctors/{fileName}";
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    UserName = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    CountryCode = dto.CountryCode,
                    PreferredLanguage = dto.PreferredLanguage,
                    Gender = dto.Gender,
                    IsActive = false
                };

                var result =
                    await _userManager.CreateAsync(
                        user,
                        dto.Password);

                if (!result.Succeeded)
                    throw new Exception(
                        result.Errors.First().Description);

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        Roles.Clinician);

                if (!roleResult.Succeeded)
                    throw new Exception(
                        roleResult.Errors.First().Description);

                var doctor = new DoctorsProfile
                {
                    FullName = dto.FullName,
                    UserId = user.Id,

                    DoctorType = dto.DoctorType,

                    Specialization =
                        dto.Specialization,

                    Bio = dto.Bio,

                    YearsOfExperience =
                        dto.YearsOfExperience,

                    DocumentsPdfUrl =
                        pdfPath,

                    ApprovalStatus =
                        ApprovalStatus.Pending
                };

                await _context.DoctorsProfiles
                    .AddAsync(doctor);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AuthResponseDTO
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    ProfilePicture = user.ProfilePicture,
                    Token = "",
                    Expiration = DateTime.UtcNow
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                var createdUser =
                    await _userManager.FindByEmailAsync(dto.Email);

                if (createdUser != null)
                {
                    await _userManager.DeleteAsync(createdUser);
                }

                throw;
            }
        }

        // ─── Forgot Password ──────────────────────────────
        public async Task ForgotPasswordAsync(ForgotPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // ✅ Debug مؤقت
            if (user == null)
                throw new Exception($"No user found with email: {dto.Email}");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(dto.Email);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

            try
            {
                await _emailService.SendPasswordResetEmailAsync(dto.Email, resetLink);
            }
            catch (Exception ex)
            {
                throw new Exception($"SMTP Error: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        // ─── Reset Password ───────────────────────────────
        public async Task ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                throw new Exception("User not found.");

            var decodedToken = Uri.UnescapeDataString(dto.Token);

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                dto.NewPassword);

            if (!result.Succeeded)
                throw new Exception(
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));
        }
    }
}