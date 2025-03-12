using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record IngredientUnit
{
    public required IngredientUnitName Name { get; init; }
    public IngredientUnitAbbreviation? Abbreviation { get; init; }
    public required IngredientUnitType UnitType { get; init; }
}
