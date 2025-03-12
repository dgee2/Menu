using MenuApi.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace MenuApi.ViewModel;

public class IngredientUnit(IngredientUnitName name, IngredientUnitAbbreviation? abbreviation, IngredientUnitType type)
{
    [Required]
    public IngredientUnitName Name { get; } = name;
    [Required]
    public IngredientUnitAbbreviation? Abbreviation { get; } = abbreviation;
    [Required]
    public IngredientUnitType Type { get; } = type;
}