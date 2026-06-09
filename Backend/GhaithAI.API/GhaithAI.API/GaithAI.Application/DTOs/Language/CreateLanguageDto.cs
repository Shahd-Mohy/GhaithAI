namespace GhaithAI.GaithAI.Application.DTOs.Language
{
    public class CreateLanguageDto
    {
        [Required(ErrorMessage = "Language name is required.")]
        [StringLength(100, ErrorMessage = "Language name cannot exceed 100 characters.")]
        public string LanguageName { get; set; }
    }
}
