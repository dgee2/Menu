using MenuApi.StrongIds;

namespace MenuApi.DBModel;

public sealed record Ingredient
{
    public IngredientId Id { get; init; }
    public required string Name { get; init; }
    public required string Unit { get; init; }
    public required string UnitAbbreviation { get; init; }
};
