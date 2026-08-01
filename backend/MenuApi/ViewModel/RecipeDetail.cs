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
    public string AccessScope { get; init; }

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

    public int? TotalTimeMinutes { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}
