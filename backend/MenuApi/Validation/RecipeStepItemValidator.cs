using FluentValidation;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class RecipeStepItemValidator : AbstractValidator<RecipeStepItem>
{
    public RecipeStepItemValidator()
    {
        RuleFor(x => x.InstructionText)
            .NotNull().WithMessage("'InstructionText' must not be empty.")
            .NotEmpty().WithMessage("'InstructionText' must not be empty.")
            .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("'InstructionText' must not be whitespace.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("'SortOrder' must be a non-negative number.");

        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200).WithMessage("'Title' must be at most 200 characters.");
        });

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes!.Value)
                .GreaterThan(0)
                .OverridePropertyName("DurationMinutes")
                .WithMessage("'DurationMinutes' must be greater than '0'.");
        });
    }
}
