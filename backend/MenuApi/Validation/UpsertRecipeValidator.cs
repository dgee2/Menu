using FluentValidation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class UpsertRecipeValidator : AbstractValidator<UpsertRecipe>
{
    public UpsertRecipeValidator()
    {
        Include(VogenValidationRules.StringRules<UpsertRecipe, RecipeTitle>(
            x => x.Title, x => x.Title.Value,
            x => x.Title.IsInitialized(), "Title", 200));

        RuleFor(x => x.AccessScope)
            .NotNull().WithMessage("'AccessScope' must not be empty.")
            .Must(s => RecipeAccessScope.AllValues.Contains(s))
            .WithMessage($"'AccessScope' must be one of: {string.Join(", ", RecipeAccessScope.AllValues)}.");

        RuleFor(x => x.Ingredients)
            .NotNull()
            .WithMessage("'Ingredients' must not be empty.");

        RuleFor(x => x.Ingredients)
            .Must(i => i is { Count: > 0 })
            .When(x => x.Ingredients is not null)
            .WithMessage("'Ingredients' must not be empty.");

        RuleForEach(x => x.Ingredients)
            .SetValidator(new RecipeIngredientItemValidator());

        RuleFor(x => x.Steps)
            .NotNull()
            .WithMessage("'Steps' must not be null.");

        RuleForEach(x => x.Steps)
            .SetValidator(new RecipeStepItemValidator());
    }
}
