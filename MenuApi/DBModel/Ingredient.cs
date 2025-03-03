using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record Ingredient
{
    public IngredientId Id { get; init; }
    public required IngredientName Name { get; init; }
    public required IngredientUnitName Unit { get; init; }
    public required IngredientUnitAbbreviation UnitAbbreviation { get; init; }
    public required IngredientUnitType UnitType { get; init; }
};
