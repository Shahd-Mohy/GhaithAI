namespace GhaithAI.GaithAI.Application.DTOs.Language
{
    public class UpdateLanguageDto
    {
        [Required(ErrorMessage = "Language Code is required for update.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Language name is required.")]
        [StringLength(50, ErrorMessage = "Language name cannot exceed 100 characters.")]
        public string LanguageName { get; set; }
    }
}
