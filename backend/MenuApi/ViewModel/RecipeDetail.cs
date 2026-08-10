using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class RecipeDetail
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public RecipeId Id { get; init; }

    [Required]
    public RecipeTitle Title { get; init; }

    [Required]
    public RecipeAccessScope AccessScope { get; init; }

    [Required]
    public IEnumerable<RecipeIngredientItem> Ingredients { get; set; }

    [Required]
    public IEnumerable<RecipeStepItem> Steps { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public MenuUserId? OwnerUserId { get; init; }

    public string? Summary { get; init; }

    public int? Servings { get; init; }

    public string? YieldText { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    /// <summary>The caller's explicit override, or <see langword="null"/> when the total is derived.</summary>
    public int? TotalTimeMinutes { get; init; }

    /// <summary>
    /// The total time to display. Carried alongside <see cref="TotalTimeMinutes"/> rather than
    /// replacing it so an editor can tell "derived" from "explicitly set" and offer the derived
    /// figure as a placeholder rather than as a value.
    /// </summary>
    public int? EffectiveTotalTimeMinutes =>
        RecipeTiming.Effective(TotalTimeMinutes, PrepTimeMinutes, CookTimeMinutes);

    /// <summary>
    /// Whether the caller may edit this recipe. Server-computed, because ownership stops being the
    /// same thing as editability once recipe sharing lands.
    /// </summary>
    [Required]
    public bool CanEdit { get; set; }

    /// <summary>Whether the caller may delete this recipe. Server-computed, as with <see cref="CanEdit"/>.</summary>
    [Required]
    public bool CanDelete { get; set; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
