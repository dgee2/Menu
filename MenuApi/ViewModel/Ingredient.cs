using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class Ingredient
{
    public Ingredient(int id, string name, IEnumerable<IngredientUnit> units)
    {
        Id = id;
        Name = name;
        Units = units;
    }

    [Required]
    public int Id { get; }

    [Required]
    public string Name { get; }

    [Required]
    public IEnumerable<IngredientUnit> Units { get; }
}

public class IngredientUnit
{
    public IngredientUnit(string name, string abbreviation)
    {
        Name = name;
        Abbreviation = abbreviation;
    }

    [Required]
    public string Name { get; }
    [Required]
    public string Abbreviation { get; }
}