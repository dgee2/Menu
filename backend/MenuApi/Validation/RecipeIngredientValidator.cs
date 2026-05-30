using FluentValidation;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class RecipeIngredientValidator : AbstractValidator<RecipeIngredient>
{
    public RecipeIngredientValidator()
    {
        RuleFor(x => x.IngredientText)
            .NotNull().WithMessage("'IngredientText' must not be empty.")
            .NotEmpty().WithMessage("'IngredientText' must not be empty.")
            .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("'IngredientText' must not be whitespace.")
            .MaximumLength(200).WithMessage("'IngredientText' must be at most 200 characters.");

        RuleFor(x => x.MeasureText)
            .NotNull().WithMessage("'MeasureText' must not be empty.")
            .NotEmpty().WithMessage("'MeasureText' must not be empty.")
            .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("'MeasureText' must not be whitespace.")
            .MaximumLength(100).WithMessage("'MeasureText' must be at most 100 characters.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("'SortOrder' must be a non-negative number.");

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount!.Value)
                .GreaterThan(0)
                .OverridePropertyName("Amount")
                .WithMessage("'Amount' must be greater than '0'.");

            RuleFor(x => x.Amount!.Value)
                .PrecisionScale(10, 4, ignoreTrailingZeros: true)
                .OverridePropertyName("Amount")
                .WithMessage("'Amount' must have at most 10 digits total, with 4 decimal places.");
        });
    }
}
