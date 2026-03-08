namespace MenuDB.Data;

public class UnitEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Abbreviation { get; set; }
    public int UnitTypeId { get; set; }
    public UnitTypeEntity UnitType { get; set; } = null!;
    public ICollection<IngredientUnitEntity> IngredientUnits { get; set; } = [];
}
