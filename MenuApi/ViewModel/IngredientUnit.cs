using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class IngredientUnit(string name, string abbreviation, string type)
{
    [Required]
    public string Name { get; } = name;
    [Required]
    public string Abbreviation { get; } = abbreviation;
    [Required]
    public string Type { get; } = type;
}