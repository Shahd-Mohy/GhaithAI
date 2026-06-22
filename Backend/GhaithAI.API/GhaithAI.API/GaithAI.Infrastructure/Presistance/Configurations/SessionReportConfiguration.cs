using GhaithAI.GaithAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class SessionReportConfiguration : IEntityTypeConfiguration<SessionReport>
    {
        public void Configure(EntityTypeBuilder<SessionReport> builder)
        {
            builder.ToTable("SessionReports");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(r => r.RiskTier)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.HasIndex(r => r.RiskTier);
            builder.HasIndex(r => r.SiPresent);

            // ── Risk detail fields ────────────────────────────────────────────
            builder.Property(r => r.SuicidalIdeationDetails)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired(false);

            builder.Property(r => r.RiskNarrative)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired(false);

            // ── SOAP fields ───────────────────────────────────────────────────
            builder.Property(r => r.SoapSubjective).HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(r => r.SoapObjective).HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(r => r.SoapAssessment).HasColumnType("nvarchar(max)").IsRequired(false);
            builder.Property(r => r.SoapPlan).HasColumnType("nvarchar(max)").IsRequired(false);

            // ── Chief Complaint fields ────────────────────────────────────────
            builder.Property(r => r.ChiefComplaintPrimary)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired(false);

            builder.Property(r => r.ChiefComplaintDuration)
                   .HasMaxLength(500)
                   .IsRequired(false);

            // ── Clinical Formulation ──────────────────────────────────────────
            // Stored as a JSON array string e.g. ["Burnout", "Recurrent MDD"]
            builder.Property(r => r.DifferentialConsiderations)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired(false);

            // ── Full AI output (for PDF/versioning only) ──────────────────────
            builder.Property(r => r.ReportJson)
                   .HasColumnType("nvarchar(max)")
                   .IsRequired();

            // ── Relationships ─────────────────────────────────────────────────
            builder.HasOne(r => r.ClinicalSession)
                   .WithMany()
                   .HasForeignKey(r => r.SessionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Patient)
                   .WithMany()
                   .HasForeignKey(r => r.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Clinician)
                   .WithMany()
                   .HasForeignKey(r => r.ClinicianId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

