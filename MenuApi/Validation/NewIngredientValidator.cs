using FluentValidation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class NewIngredientValidator : AbstractValidator<NewIngredient>
{
    public NewIngredientValidator()
    {
        Include(VogenValidationRules.StringRules<NewIngredient, IngredientName>(
            x => x.Name, x => x.Name.Value,
            x => x.Name.IsInitialized(), "Name", 50));

        RuleFor(x => x.UnitIds)
            .NotNull()
            .WithMessage("'Unit Ids' must not be empty.");

        RuleFor(x => x.UnitIds)
            .Must(u => u is { Count: > 0 })
            .When(x => x.UnitIds is not null)
            .WithMessage("'Unit Ids' must not be empty.");

        RuleForEach(x => x.UnitIds)
            .GreaterThan(0)
            .WithMessage("Each unit ID must be greater than 0.");
    }
}
