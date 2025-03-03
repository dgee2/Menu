using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class RecipeIngredient
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    [Required]
    public IngredientName Name { get; init; }

    [Required]
    public IngredientUnitName Unit { get; init; }

    [Required]
    public IngredientAmount Amount { get; init; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
