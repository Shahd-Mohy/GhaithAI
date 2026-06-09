namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorDefaultScheduleConfiguration : IEntityTypeConfiguration<DoctorDefaultSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorDefaultSchedule> builder)
        {
            builder.ToTable("DoctorDefaultSchedules");
            builder.HasKey(dds => dds.Id);
            builder.Property(dds => dds.StartTime).IsRequired();  
            builder.Property(dds => dds.EndTime) .IsRequired(); 
            builder.Property(dds => dds.IsActive).IsRequired().HasDefaultValue(true); 
            builder.Property(dds => dds.DayOfWeek).HasConversion<string>() .HasMaxLength(20) .IsRequired();
 
            builder.HasOne(dds => dds.Clinic).WithMany(c => c.DefaultSchedules)  
                .HasForeignKey(dds => dds.ClinicId).OnDelete(DeleteBehavior.Cascade); 
                
        }
    }
}
