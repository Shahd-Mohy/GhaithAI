namespace GhaithAI.GaithAI.Application.DTOs.SelfHelp
{
    public class UserSelfHelpDetailsResponseDto : UserSelfHelpResponseDto
    {
        public List<string> ExerciseTips { get; set; } = new();
    }
}
