namespace MenuDB.Data;

public class RecipeIngredientEntity
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public RecipeEntity Recipe { get; set; } = null!;

    public int SortOrder { get; set; }

    public required string IngredientText { get; set; }

    public required string MeasureText { get; set; }

    public string? SectionTitle { get; set; }

    public decimal? Amount { get; set; }

    public string? UnitText { get; set; }

    public string? PreparationText { get; set; }

    public bool IsOptional { get; set; }

    public int? CanonicalIngredientId { get; set; }

    public IngredientEntity? CanonicalIngredient { get; set; }

    public int? CanonicalUnitId { get; set; }

    public UnitEntity? CanonicalUnit { get; set; }
}
