namespace MenuDB.Data;

public class RecipeEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int? OwnerUserId { get; set; }
    public MenuUserEntity? Owner { get; set; }
    public byte AccessScopeId { get; set; }
    public RecipeAccessScopeEntity? AccessScope { get; set; }
    public string? Summary { get; set; }
    public int? Servings { get; set; }
    public string? YieldText { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? TotalTimeMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<RecipeIngredientEntity> RecipeIngredients { get; set; } = [];

    public ICollection<RecipeStepEntity> RecipeSteps { get; set; } = [];
}
