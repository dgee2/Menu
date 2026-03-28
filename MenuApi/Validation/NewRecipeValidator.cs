using FluentValidation;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class NewRecipeValidator : AbstractValidator<NewRecipe>
{
    public NewRecipeValidator()
    {
        RuleFor(x => x.Name.Value)
            .NotEmpty()
            .OverridePropertyName("Name")
            .WithMessage("'Name' must not be empty.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(500)
            .OverridePropertyName("Name")
            .WithMessage("'Name' must be 500 characters or fewer.");

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
