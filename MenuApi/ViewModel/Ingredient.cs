using MenuApi.StrongIds;
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

public class IngredientUnit(string name, string abbreviation)
{
    [Required]
    public string Name { get; } = name;
    [Required]
    public string Abbreviation { get; } = abbreviation;
}