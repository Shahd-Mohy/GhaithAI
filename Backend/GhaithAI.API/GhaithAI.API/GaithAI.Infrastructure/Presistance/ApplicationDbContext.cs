
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using System.Linq.Expressions;

namespace GhaithAI.API.Data
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. SCAN AND APPLY ALL SEPARATE CONFIGURATION CLASSES AUTOMATICALLY
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // 2. DYNAMIC GLOBAL QUERY FILTER FOR SOFT DELETE
            // Automatically appends "WHERE IsDeleted = false" to any entity inheriting from AuditableEntity
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var isDeletedProperty = entityType.FindProperty("IsDeleted");
                if (isDeletedProperty != null && isDeletedProperty.ClrType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyAccess = Expression.Property(parameter, isDeletedProperty.PropertyInfo);
                    var notExpression = Expression.Not(propertyAccess);
                    var lambda = Expression.Lambda(notExpression, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        // 3. AUTOMATED AUDIT ENGINE & SOFT-DELETE INTERCEPTOR
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();

                // Handle Auditable & Base Entities during Creation
                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entityType.GetProperty("CreatedAt");
                    if (createdAtProp != null && createdAtProp.CanWrite)
                    {
                        createdAtProp.SetValue(entry.Entity, DateTime.UtcNow);
                    }
                }

                // Handle Auditable Entities during Modification
                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entityType.GetProperty("UpdatedAt");
                    if (updatedAtProp != null && updatedAtProp.CanWrite)
                    {
                        updatedAtProp.SetValue(entry.Entity, DateTime.UtcNow);
                    }

                    var updatedByProp = entityType.GetProperty("UpdatedBy");
                    if (updatedByProp != null && updatedByProp.CanWrite)
                    {
                        // Placeholder: Can be integrated with an IUserContext service later
                        updatedByProp.SetValue(entry.Entity, "System");
                    }
                }

                // Intercept Hard Delete and Convert to Soft Delete
                if (entry.State == EntityState.Deleted)
                {
                    var isDeletedProp = entityType.GetProperty("IsDeleted");
                    if (isDeletedProp != null && isDeletedProp.CanWrite)
                    {
                        // Change state from Deleted to Modified
                        entry.State = EntityState.Modified;
                        isDeletedProp.SetValue(entry.Entity, true);

                        var deletedAtProp = entityType.GetProperty("DeletedAt");
                        if (deletedAtProp != null && deletedAtProp.CanWrite)
                        {
                            deletedAtProp.SetValue(entry.Entity, DateTime.UtcNow);
                        }
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}