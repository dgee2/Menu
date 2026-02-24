namespace MenuDB.Data;

public class IngredientEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<IngredientUnitEntity> IngredientUnits { get; set; } = [];
}
