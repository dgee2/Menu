namespace MenuApi.DBModel;

public sealed class GetRecipeIngredient
{
    public required string IngredientName { get; init; }
    public decimal Amount { get; init; }
    public required string UnitName { get; init; }
    public required string UnitAbbreviation { get; init; }
}
