namespace GhaithAI.GaithAI.Application.DTOs.ClinicPatient
{
    public class UpdateClinicPatientDto
    {
        [Required(ErrorMessage = "Patient ID is required for update.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Doctor ID is required.")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Patient full name is required.")]
        [StringLength(150, ErrorMessage = "Patient name cannot exceed 150 characters.")]
        public string PatientFullName { get; set; }

        [Required(ErrorMessage = "Patient phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(11, ErrorMessage = "Phone number must equal 20 characters.")]
        public string PatientPhone { get; set; }

        public string? Notes { get; set; }
    }
}
