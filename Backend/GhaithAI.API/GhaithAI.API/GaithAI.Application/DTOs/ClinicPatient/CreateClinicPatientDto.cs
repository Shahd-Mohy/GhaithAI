namespace GhaithAI.GaithAI.Application.DTOs.ClinicPatient
{
    public class CreateClinicPatientDto
    {
        [Required(ErrorMessage = "Doctor ID is required.")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Patient full name is required.")]
        [StringLength(150, ErrorMessage = "Patient name cannot exceed 150 characters.")]
        public string PatientFullName { get; set; }

        [Required(ErrorMessage = "Patient phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(11, ErrorMessage = "Phone number must equal 11 characters.")]
        public string PatientPhone { get; set; }

        public string? Notes { get; set; }
    }
}
