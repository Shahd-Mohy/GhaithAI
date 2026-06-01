using FluentValidation;
using GhaithAI.API.Constants;
using GhaithAI.API.DTOs.Mood;
namespace GhaithAI.API.Validators
{
    public class MoodValidator : AbstractValidator<CreateMoodLogDTO>
    {
        public MoodValidator()
        {
            RuleFor(x => x.MoodScore)
                .InclusiveBetween(1, 5)
                .WithMessage("Mood score must be between 1 (Very Low) and 5 (Great).");


            RuleFor(x => x.EmotionTags)
                .MaximumLength(500)
                .WithMessage("Emotion tags are too long.")
                .Must(BeValidTags)
                .WithMessage($"Emotion tags must be from the allowed list: " +
                             $"{string.Join(", ", EmotionTypes.All)}.")
                .When(x => !string.IsNullOrWhiteSpace(x.EmotionTags));

            RuleFor(x => x.StressLevel)
                .InclusiveBetween(0, 10)
                .WithMessage("Stress level must be between 0 and 10.");

            RuleFor(x => x.SleepQuality)
                .InclusiveBetween(0, 10)
                .WithMessage("Sleep quality must be between 0 and 10.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters.")
                .When(x => x.Notes != null);
        }

        private bool BeValidTags(string? tags)
        {
            var parsed = EmotionTypes.Parse(tags);
            return parsed.All(tag => EmotionTypes.All.Contains(tag));
        }
    }
}
