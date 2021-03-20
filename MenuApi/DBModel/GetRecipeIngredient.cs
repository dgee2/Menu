namespace MenuApi.DBModel
{
    public sealed record GetRecipeIngredient(string IngredientName, decimal Amount, string UnitName, string UnitAbbreviation);
}
