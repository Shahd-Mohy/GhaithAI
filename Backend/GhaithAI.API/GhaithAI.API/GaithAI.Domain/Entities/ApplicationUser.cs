using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GhaithAI.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string? CountryCode { get; set; }

        public string PreferredLanguage { get; set; } = "en";

        public bool MemoryEnabled { get; set; } = false;

        public bool MFAEnabled { get; set; } = false;

        public string? ProfilePicture { get; set; }

        public bool AcceptedAiChat { get; set; } = false;

        public bool AcceptedMoodTracking { get; set; } = false;

        public bool AcceptedDataCollection { get; set; } = false;

        public bool AcceptedTerms { get; set; } = false;

        public bool AcceptedPrivacyPolicy { get; set; } = false;

        public DateTime? ConsentedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public Country Country { get; set; }

        public ICollection<ChatSession> ChatSessions { get; set; }

        public ICollection<MoodLog> MoodLogs { get; set; }

        public ICollection<JournalEntry> JournalEntries { get; set; }

        public ICollection<UserActivity> UserActivities { get; set; }
        
        public ICollection<EmergencyContact> EmergencyContacts { get; set; }

        public ICollection<WeeklyInsightReport> WeeklyInsightReports { get; set; }

        public ICollection<DoctorReview> DoctorReviews { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public UserAssessment UserAssessment { get; set; }
        public DoctorsProfile? DoctorsProfile { get; set; }
        public string? GoogleId { get; set; }

        public bool IsGoogleAccount { get; set; } = false;

        public Gender Gender { get; set; }
    }
}