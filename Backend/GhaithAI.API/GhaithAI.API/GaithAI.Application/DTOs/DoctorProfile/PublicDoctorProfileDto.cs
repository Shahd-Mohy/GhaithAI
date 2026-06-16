namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class PublicDoctorProfileDto : PublicDoctorCardDto
    {
        public List<DefaultScheduleDto> WeeklySchedule { get; set; } = new();
    }
}
