using Riok.Mapperly.Abstractions;

namespace MenuApi.MappingProfiles;

[Mapper]
public static partial class ViewModelMapper
{
    public static partial ViewModel.Recipe Map(DBModel.Recipe recipe);

    public static partial IEnumerable<ViewModel.Recipe> Map(IEnumerable<DBModel.Recipe> recipes);

    public static partial ViewModel.RecipeIngredient Map(DBModel.RecipeIngredient recipeIngredient);

    public static partial IEnumerable<ViewModel.RecipeIngredient> Map(IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    public static partial DBModel.RecipeIngredient Map(ViewModel.RecipeIngredient recipeIngredient);

    public static partial IEnumerable<DBModel.RecipeIngredient> Map(IEnumerable<ViewModel.RecipeIngredient> recipeIngredients);

    public static partial ViewModel.RecipeStep Map(DBModel.RecipeStep recipeStep);

    public static partial IEnumerable<ViewModel.RecipeStep> Map(IEnumerable<DBModel.RecipeStep> recipeSteps);

    public static partial DBModel.RecipeStep Map(ViewModel.RecipeStep recipeStep);

    public static partial IEnumerable<DBModel.RecipeStep> Map(IEnumerable<ViewModel.RecipeStep> recipeSteps);

    [MapProperty(nameof(DBModel.IngredientUnit.UnitType), nameof(ViewModel.IngredientUnit.Type))]
    public static partial ViewModel.IngredientUnit Map(DBModel.IngredientUnit ingredientUnit);

    public static partial IEnumerable<ViewModel.IngredientUnit> Map(IEnumerable<DBModel.IngredientUnit> ingredientUnit);

    [MapperIgnoreTarget(nameof(ViewModel.FullRecipe.Ingredients))]
    public static partial ViewModel.FullRecipe? MapToFullRecipe(DBModel.Recipe? recipe);
}
