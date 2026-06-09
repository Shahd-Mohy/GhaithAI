namespace GhaithAI.GaithAI.Application.DTOs.Specialty
{
    public class CreateSpecialtyDto
    {
        [Required(ErrorMessage = "Specialty name is required.")]
        [StringLength(150, ErrorMessage = "Specialty name cannot exceed 150 characters.")]
        public string SpecialtyName { get; set; }
    }
}
