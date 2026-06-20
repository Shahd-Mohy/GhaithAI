namespace GhaithAI.GaithAI.Application.DTOs.DoctorProfile
{
    public class DoctorPatientsDashboardDto
    {
        public int TotalPatients { get; set; }
        public int HighRiskCount { get; set; }
        public int SessionsThisWeek { get; set; }
        public int TotalRiskAlerts { get; set; }
        public int TotalFilteredPatients { get; set; } 
        public List<PatientListItemDto> Patients { get; set; } = new();
    }
    public class PatientListItemDto
    {
        public string PatientId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string RiskLevel { get; set; } 

        public DateTime? LastBookingDate { get; set; }
        public int TotalBookingsCount { get; set; }
    }
}
