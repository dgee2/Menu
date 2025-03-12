using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class Ingredient
{
    [Required]
    public required IngredientId Id { get; init; }

    [Required]
    public required IngredientName Name { get; init; }

    [Required]
    public required IEnumerable<IngredientUnit> Units { get; init; }
}
