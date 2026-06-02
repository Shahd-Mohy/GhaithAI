using GhaithAI.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GhaithAI.API.Presistance
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<ChatSession> ChatSessions { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<MoodLog> MoodLogs { get; set; }

        public DbSet<JournalEntry> JournalEntries { get; set; }

        public DbSet<RiskEvent> RiskEvents { get; set; }

        public DbSet<EmergencyContact> EmergencyContacts { get; set; }

        public DbSet<SelfHelpContent> SelfHelpContents { get; set; }

        public DbSet<UserActivity> UserActivities { get; set; }

        public DbSet<WeeklyInsightReport> WeeklyInsightReports { get; set; }

        public DbSet<CrisisResourceConfig> CrisisResourceConfigs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=GhaithAI_DB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ChatMessage>()
                .Property(p => p.SentimentScore)
                .HasColumnType("decimal(18,4)");

            builder.Entity<RiskEvent>()
                .Property(p => p.ConfidenceScore)
                .HasColumnType("decimal(18,4)");

            builder.Entity<WeeklyInsightReport>()
                .Property(p => p.AvgMoodScore)
                .HasColumnType("decimal(18,4)");
            builder.Entity<RiskEvent>()
                .HasOne(r => r.ChatSession)
                .WithMany(c => c.RiskEvents)
                .HasForeignKey(r => r.SessionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RiskEvent>()
                .HasOne(r => r.ChatMessage)
                .WithMany(m => m.RiskEvents)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Country>()
                .HasKey(c => c.CountryCode);

            builder.Entity<Country>()
                .HasIndex(c => c.CountryName)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CountryCode);

            builder.Entity<CrisisResourceConfig>()
                .HasOne(c => c.Country)
                .WithMany(c => c.CrisisResourceConfigs)
                .HasForeignKey(c => c.CountryCode);

            builder.Entity<WeeklyInsightReport>()
                .HasIndex(r => new { r.UserId, r.WeekStart })
                .IsUnique();
        }
    }
}