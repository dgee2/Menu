using FluentValidation;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class RecipeIngredientValidator : AbstractValidator<RecipeIngredient>
{
    public RecipeIngredientValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .WithName("Name")
            .WithMessage("'Name' must not be empty.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(50)
            .WithName("Name")
            .WithMessage("'Name' must be 50 characters or fewer.");

        RuleFor(x => x.Unit.Value)
            .NotEmpty()
            .WithName("Unit")
            .WithMessage("'Unit' must not be empty.");

        RuleFor(x => x.Unit.Value)
            .MaximumLength(50)
            .WithName("Unit")
            .WithMessage("'Unit' must be 50 characters or fewer.");

        RuleFor(x => x.Amount.Value)
            .GreaterThan(0)
            .WithName("Amount")
            .WithMessage("'Amount' must be greater than '0'.");

        RuleFor(x => x.Amount.Value)
            .PrecisionScale(10, 4, ignoreTrailingZeros: true)
            .WithName("Amount")
            .WithMessage("'Amount' must have at most 10 digits total, with 4 decimal places.");
    }
}
