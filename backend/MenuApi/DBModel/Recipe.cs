using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record Recipe
{
    public required RecipeId Id { get; init; }
    public required RecipeName Name { get; init; }
}
