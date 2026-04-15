using FluentValidation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class RecipeIngredientValidator : AbstractValidator<RecipeIngredient>
{
    public RecipeIngredientValidator()
    {
        Include(VogenValidationRules.StringRules<RecipeIngredient, IngredientName>(
            x => x.Name, x => x.Name.Value,
            x => x.Name.IsInitialized(), "Name", 50));

        Include(VogenValidationRules.StringRules<RecipeIngredient, IngredientUnitName>(
            x => x.Unit, x => x.Unit.Value,
            x => x.Unit.IsInitialized(), "Unit", 50));

        RuleFor(x => x.Amount)
            .Must(amount => amount.IsInitialized())
            .OverridePropertyName("Amount")
            .WithMessage("'Amount' must not be empty.");

        RuleFor(x => x.Amount.Value)
            .GreaterThan(0)
            .OverridePropertyName("Amount")
            .WithMessage("'Amount' must be greater than '0'.")
            .When(x => x.Amount.IsInitialized());

        RuleFor(x => x.Amount.Value)
            .PrecisionScale(10, 4, ignoreTrailingZeros: true)
            .OverridePropertyName("Amount")
            .WithMessage("'Amount' must have at most 10 digits total, with 4 decimal places.")
            .When(x => x.Amount.IsInitialized());
    }
}
