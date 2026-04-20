using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed class GetRecipeIngredient
{
    public required IngredientName IngredientName { get; init; }
    public required IngredientAmount Amount { get; init; }
    public required IngredientUnitName UnitName { get; init; }
    public required IngredientUnitAbbreviation UnitAbbreviation { get; init; }
}
