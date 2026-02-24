namespace MenuApi.Data;

public class RecipeEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<RecipeIngredientEntity> RecipeIngredients { get; set; } = [];
}
