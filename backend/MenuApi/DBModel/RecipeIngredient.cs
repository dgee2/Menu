using MenuApi.ValueObjects;

namespace MenuApi.DBModel;

public sealed record RecipeIngredient(IngredientName IngredientName, IngredientAmount Amount, IngredientUnitName UnitName);
