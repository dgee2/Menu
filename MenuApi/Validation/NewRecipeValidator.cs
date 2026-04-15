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
    }
}
