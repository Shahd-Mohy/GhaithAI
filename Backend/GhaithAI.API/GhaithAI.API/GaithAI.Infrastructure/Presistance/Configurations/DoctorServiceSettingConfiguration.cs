namespace GhaithAI.GaithAI.Infrastructure.Presistance.Configurations
{
    public class DoctorServiceSettingConfiguration : IEntityTypeConfiguration<DoctorServiceSetting>
    {
        public void Configure(EntityTypeBuilder<DoctorServiceSetting> builder)
        {
            builder.ToTable("DoctorServiceSettings");
            builder.HasKey(dss => dss.Id);

            builder.Property(dss => dss.FeePerSession).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(dss => dss.SessionDurationMinutes).IsRequired();
            builder.Property(dss => dss.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(dss => dss.AvailableSessionType).HasConversion<string>().HasMaxLength(30).IsRequired();

            builder.HasOne(dss => dss.Doctor)
                .WithOne(d => d.ServiceSetting)
                .HasForeignKey<DoctorServiceSetting>(dss => dss.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
