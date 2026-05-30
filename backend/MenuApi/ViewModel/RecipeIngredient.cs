namespace MenuApi.ViewModel;

public class RecipeIngredient
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public string IngredientText { get; init; }

    public string MeasureText { get; init; }
#pragma warning restore CS8618

    public int SortOrder { get; init; }

    public string? SectionTitle { get; init; }

    public decimal? Amount { get; init; }

    public string? UnitText { get; init; }

    public string? PreparationText { get; init; }

    public bool IsOptional { get; init; }

    public int? CanonicalIngredientId { get; init; }

    public int? CanonicalUnitId { get; init; }
}
