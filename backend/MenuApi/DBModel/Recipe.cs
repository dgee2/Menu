using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record Recipe
{
    public RecipeId Id { get; init; }
    public required RecipeTitle Title { get; init; }
    public required string AccessScope { get; init; }
    public MenuUserId? OwnerUserId { get; init; }
    public string? Summary { get; init; }
    public int? Servings { get; init; }
    public string? YieldText { get; init; }
    public int? PrepTimeMinutes { get; init; }
    public int? CookTimeMinutes { get; init; }
    public int? TotalTimeMinutes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
