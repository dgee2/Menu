namespace MenuDB.Data;

/// <summary>
/// Lookup rows backing <c>MenuApi.ValueObjects.RecipeAccessScope</c>. Ids are assigned by hand and
/// must stay in step with that enum's numeric values - <c>RecipeAccessScopeDriftTests</c> guards it.
/// </summary>
public class RecipeAccessScopeEntity
{
    public byte Id { get; set; }
    public required string Name { get; set; }
    public ICollection<RecipeEntity> Recipes { get; set; } = [];
}
