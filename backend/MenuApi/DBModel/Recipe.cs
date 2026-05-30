using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record Recipe
{
    public required RecipeId Id { get; init; }
    public required RecipeTitle Title { get; init; }
}
