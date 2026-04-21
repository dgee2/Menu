namespace MenuDB.Data;

public class UnitTypeEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<UnitEntity> Units { get; set; } = [];
}
