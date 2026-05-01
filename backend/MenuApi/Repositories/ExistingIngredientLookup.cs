using MenuApi.ViewModel;

namespace MenuApi.Repositories;

public sealed record ExistingIngredientLookup(
    Ingredient Ingredient,
    IReadOnlySet<int> UnitIds);
