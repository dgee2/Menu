using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record IngredientUnit
{
    public required IngredientUnitName Name { get; init; }
    public required IngredientUnitAbbreviation Abbreviation { get; init; }
    public required IngredientUnitType UnitType { get; init; }
}
