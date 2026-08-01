using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class UpsertRecipe
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public RecipeTitle Title { get; init; }

    [Required]
    public List<RecipeIngredientItem> Ingredients { get; init; }

    [Required]
    public List<RecipeStepItem> Steps { get; init; }

    [Required]
    public string AccessScope { get; init; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public string? Summary { get; init; }

    public int? Servings { get; init; }

    public string? YieldText { get; init; }

    public int? PrepTimeMinutes { get; init; }

    public int? CookTimeMinutes { get; init; }

    public int? TotalTimeMinutes { get; init; }
}
