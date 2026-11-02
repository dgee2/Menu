namespace MenuDB.Data;

/// <summary>
/// Denormalized read model used by SQL Server full-text search. Recipe is the source of truth.
/// </summary>
public class RecipeSearchIndexEntity
{
    public Guid RecipeId { get; set; }

    public RecipeEntity? Recipe { get; set; }

    public required string Title { get; set; }

    public string? SummaryText { get; set; }

    public string? IngredientsText { get; set; }

    public string? StepsText { get; set; }

    public Guid OwnerUserId { get; set; }

    public MenuUserEntity? Owner { get; set; }

    public required string AccessScope { get; set; }

    public bool IsPublishedExternally { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
