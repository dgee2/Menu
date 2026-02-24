namespace MenuApi.Data;

public class IngredientUnitEntity
{
    public int IngredientId { get; set; }
    public IngredientEntity Ingredient { get; set; } = null!;
    public int UnitId { get; set; }
    public UnitEntity Unit { get; set; } = null!;
}
