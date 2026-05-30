namespace MenuApi.DBModel;

public sealed record RecipeIngredient(
    int SortOrder,
    string IngredientText,
    string MeasureText,
    string? SectionTitle,
    decimal? Amount,
    string? UnitText,
    string? PreparationText,
    bool IsOptional,
    int? CanonicalIngredientId,
    int? CanonicalUnitId);
