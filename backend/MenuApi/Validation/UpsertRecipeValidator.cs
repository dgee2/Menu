using FluentValidation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Validation;

public class UpsertRecipeValidator : AbstractValidator<UpsertRecipe>
{
    // YieldText mirrors its nvarchar(100) column: unvalidated, a 101-character yield reaches SQL
    // Server as a truncation error and surfaces to the caller as a 500. Summary's column is
    // nvarchar(max), so this bound is a sanity limit rather than a schema one.
    private const int SummaryMaxLength = 4000;
    private const int YieldTextMaxLength = 100;

    // Two weeks. High enough for cured meat and sourdough starters, low enough that an obvious typo
    // is caught rather than persisted.
    private const int MaxDurationMinutes = 60 * 24 * 14;
    private const int MaxServings = 1000;

    public UpsertRecipeValidator()
    {
        Include(VogenValidationRules.StringRules<UpsertRecipe, RecipeTitle>(
            x => x.Title, x => x.Title.Value,
            x => x.Title.IsInitialized(), "Title", 200));

        RuleFor(x => x.AccessScope)
            .IsInEnum()
            .WithMessage($"'AccessScope' must be one of: {string.Join(", ", Enum.GetNames<RecipeAccessScope>())}.");

        When(x => x.Summary is not null, () =>
        {
            RuleFor(x => x.Summary)
                .MaximumLength(SummaryMaxLength)
                .WithMessage($"'Summary' must be at most {SummaryMaxLength} characters.");
        });

        When(x => x.YieldText is not null, () =>
        {
            RuleFor(x => x.YieldText)
                .MaximumLength(YieldTextMaxLength)
                .WithMessage($"'YieldText' must be at most {YieldTextMaxLength} characters.");
        });

        When(x => x.Servings.HasValue, () =>
        {
            // Zero is allowed to match the editor, whose Servings field accepts it.
            RuleFor(x => x.Servings!.Value)
                .InclusiveBetween(0, MaxServings)
                .OverridePropertyName(nameof(UpsertRecipe.Servings))
                .WithMessage($"'Servings' must be between 0 and {MaxServings}.");
        });

        AddDurationRules(x => x.PrepTimeMinutes, nameof(UpsertRecipe.PrepTimeMinutes));
        AddDurationRules(x => x.CookTimeMinutes, nameof(UpsertRecipe.CookTimeMinutes));
        AddDurationRules(x => x.TotalTimeMinutes, nameof(UpsertRecipe.TotalTimeMinutes));

        RuleFor(x => x.Ingredients)
            .NotNull()
            .WithMessage("'Ingredients' must not be null.");

        RuleForEach(x => x.Ingredients)
            .SetValidator(new RecipeIngredientItemValidator());

        RuleFor(x => x.Steps)
            .NotNull()
            .WithMessage("'Steps' must not be null.");

        RuleForEach(x => x.Steps)
            .SetValidator(new RecipeStepItemValidator());
    }

    private void AddDurationRules(Func<UpsertRecipe, int?> selector, string propertyName)
    {
        When(x => selector(x).HasValue, () =>
        {
            RuleFor(x => selector(x)!.Value)
                .InclusiveBetween(0, MaxDurationMinutes)
                .OverridePropertyName(propertyName)
                .WithMessage($"'{propertyName}' must be between 0 and {MaxDurationMinutes} minutes.");
        });
    }
}
