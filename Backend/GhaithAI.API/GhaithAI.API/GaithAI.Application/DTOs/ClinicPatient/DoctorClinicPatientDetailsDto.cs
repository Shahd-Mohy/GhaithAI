namespace GhaithAI.GaithAI.Application.DTOs.ClinicPatient
{
    public class DoctorClinicPatientDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ClinicId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientPhone { get; set; }
        public string Notes { get; set; }
    }
}
