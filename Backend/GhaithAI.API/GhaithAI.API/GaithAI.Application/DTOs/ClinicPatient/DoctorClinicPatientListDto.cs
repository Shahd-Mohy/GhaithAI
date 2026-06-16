namespace GhaithAI.GaithAI.Application.DTOs.ClinicPatient
{
    public class DoctorClinicPatientListDto
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string PatientFullName { get; set; }
        public string PatientPhone { get; set; }
    }
}
