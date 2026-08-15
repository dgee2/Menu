using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class RecipeListItem
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public RecipeId Id { get; init; }

    [Required]
    public RecipeTitle Title { get; init; }

    [Required]
    public RecipeAccessScope AccessScope { get; init; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public string? Summary { get; init; }

    public int? Servings { get; init; }

    public string? YieldText { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    /// <summary>The recipe's explicit override, or <see langword="null"/> when the total is derived.</summary>
    public int? TotalTimeMinutes { get; init; }

    /// <inheritdoc cref="RecipeDetail.EffectiveTotalTimeMinutes"/>
    public int? EffectiveTotalTimeMinutes =>
        RecipeTiming.Effective(TotalTimeMinutes, PrepTimeMinutes, CookTimeMinutes);
}
