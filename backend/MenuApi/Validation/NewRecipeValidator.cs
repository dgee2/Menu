using FluentValidation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class NewRecipeValidator : AbstractValidator<NewRecipe>
{
    public NewRecipeValidator()
    {
        Include(VogenValidationRules.StringRules<NewRecipe, RecipeName>(
            x => x.Name, x => x.Name.Value,
            x => x.Name.IsInitialized(), "Name", 500));

        RuleFor(x => x.Ingredients)
            .NotNull()
            .WithMessage("'Ingredients' must not be empty.");

        RuleFor(x => x.Ingredients)
            .Must(i => i is { Count: > 0 })
            .When(x => x.Ingredients is not null)
            .WithMessage("'Ingredients' must not be empty.");

        RuleForEach(x => x.Ingredients)
            .SetValidator(new RecipeIngredientValidator());

        RuleFor(x => x.Ingredients)
            .Must(items =>
            {
                var initialized = items
                    .Where(i => i is not null && i.Name.IsInitialized() && i.Unit.IsInitialized() && i.Amount.IsInitialized())
                    .ToList();
                return !initialized
                    .GroupBy(i => new { IngredientName = i.Name.Value, UnitName = i.Unit.Value })
                    .Any(g => g.Select(i => i.Amount.Value).Distinct().Count() > 1);
            })
            .When(x => x.Ingredients is not null)
            .WithMessage("Duplicate ingredient entries with the same name and unit but different amounts are not allowed.");
    }
}
