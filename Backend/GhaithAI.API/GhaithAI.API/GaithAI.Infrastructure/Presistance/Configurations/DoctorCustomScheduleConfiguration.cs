using Microsoft.EntityFrameworkCore;

namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorCustomScheduleConfiguration : IEntityTypeConfiguration<DoctorCustomSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorCustomSchedule> builder)
        {
            builder.ToTable("DoctorCustomSchedules");
            builder.HasKey(dcs => dcs.Id);
            builder.Property(dcs => dcs.CustomDate).HasColumnType("date").IsRequired();
            builder.Property(dcs => dcs.StartTime).IsRequired(); 
            builder.Property(dcs => dcs.EndTime).IsRequired();
            builder.Property(dcs => dcs.IsOffDay).IsRequired().HasDefaultValue(false); 

            builder.HasOne(dcs => dcs.Doctor).WithMany(c => c.CustomSchedules) 
                .HasForeignKey(dcs => dcs.DoctorId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(dcs => new { dcs.DoctorId, dcs.CustomDate }).IsUnique();
        }
    }
}
