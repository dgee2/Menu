namespace MenuApi.DBModel
{
    public sealed record IngredientUnit
    {
        public required string Name { get; init; }
        public required string Abbreviation { get; init; }
    }
}
