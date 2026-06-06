using FluentValidation;
using GhaithAI.API.GaithAI.Application.DTOs.Mood;

namespace GhaithAI.GaithAI.Application.Validators
{
        public class UpdateMoodLogDTOValidator : AbstractValidator<UpdateMoodLogDTO>
        {
            public UpdateMoodLogDTOValidator()
            {
                When(x => x.MoodScore.HasValue, () =>
                {
                    RuleFor(x => x.MoodScore!.Value)
                        .InclusiveBetween(1, 5)
                        .WithMessage("Mood score must be between 1 and 5.");
                });

                When(x => x.EmotionTags != null, () =>
                {
                    RuleFor(x => x.EmotionTags)
                        .MaximumLength(500)
                        .WithMessage("Emotion tags are too long.");
                });
            }
        }
}
