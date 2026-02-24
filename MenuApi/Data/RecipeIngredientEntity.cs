namespace MenuApi.Data;

public class RecipeIngredientEntity
{
    public int RecipeId { get; set; }
    public RecipeEntity Recipe { get; set; } = null!;
    public int IngredientId { get; set; }
    public IngredientEntity Ingredient { get; set; } = null!;
    public int UnitId { get; set; }
    public UnitEntity Unit { get; set; } = null!;
    public decimal Amount { get; set; }
}
