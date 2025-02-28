using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class Ingredient(IngredientId id, string name, IEnumerable<IngredientUnit> units)
{
    [Required]
    public IngredientId Id { get; } = id;

    [Required]
    public string Name { get; } = name;

    [Required]
    public IEnumerable<IngredientUnit> Units { get; } = units;
}
