using Riok.Mapperly.Abstractions;

namespace MenuApi.MappingProfiles;

[Mapper]
public static partial class ViewModelMapper
{
    [MapperIgnoreSource(nameof(DBModel.Recipe.OwnerUserId))]
    [MapperIgnoreSource(nameof(DBModel.Recipe.CreatedAtUtc))]
    [MapperIgnoreSource(nameof(DBModel.Recipe.UpdatedAtUtc))]
    public static partial ViewModel.RecipeListItem Map(DBModel.Recipe recipe);

    public static partial IEnumerable<ViewModel.RecipeListItem> Map(IEnumerable<DBModel.Recipe> recipes);

    [MapperIgnoreTarget(nameof(DBModel.Recipe.Id))]
    [MapperIgnoreTarget(nameof(DBModel.Recipe.OwnerUserId))]
    [MapperIgnoreTarget(nameof(DBModel.Recipe.CreatedAtUtc))]
    [MapperIgnoreTarget(nameof(DBModel.Recipe.UpdatedAtUtc))]
    [MapperIgnoreSource(nameof(ViewModel.UpsertRecipe.Ingredients))]
    [MapperIgnoreSource(nameof(ViewModel.UpsertRecipe.Steps))]
    public static partial DBModel.Recipe Map(ViewModel.UpsertRecipe upsertRecipe);

    public static partial ViewModel.RecipeIngredientItem Map(DBModel.RecipeIngredient recipeIngredient);

    public static partial IEnumerable<ViewModel.RecipeIngredientItem> Map(IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    public static partial DBModel.RecipeIngredient Map(ViewModel.RecipeIngredientItem recipeIngredient);

    public static partial IEnumerable<DBModel.RecipeIngredient> Map(IEnumerable<ViewModel.RecipeIngredientItem> recipeIngredients);

    public static partial ViewModel.RecipeStepItem Map(DBModel.RecipeStep recipeStep);

    public static partial IEnumerable<ViewModel.RecipeStepItem> Map(IEnumerable<DBModel.RecipeStep> recipeSteps);

    public static partial DBModel.RecipeStep Map(ViewModel.RecipeStepItem recipeStep);

    public static partial IEnumerable<DBModel.RecipeStep> Map(IEnumerable<ViewModel.RecipeStepItem> recipeSteps);

    [MapProperty(nameof(DBModel.IngredientUnit.UnitType), nameof(ViewModel.IngredientUnit.Type))]
    public static partial ViewModel.IngredientUnit Map(DBModel.IngredientUnit ingredientUnit);

    public static partial IEnumerable<ViewModel.IngredientUnit> Map(IEnumerable<DBModel.IngredientUnit> ingredientUnit);

    [MapperIgnoreTarget(nameof(ViewModel.RecipeDetail.Ingredients))]
    [MapperIgnoreTarget(nameof(ViewModel.RecipeDetail.Steps))]
    public static partial ViewModel.RecipeDetail? MapToRecipeDetail(DBModel.Recipe? recipe);
}
