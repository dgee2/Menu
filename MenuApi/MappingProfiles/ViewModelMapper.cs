using Riok.Mapperly.Abstractions;

namespace MenuApi.MappingProfiles;

[Mapper]
public static partial class ViewModelMapper
{
    public static partial ViewModel.Recipe Map(DBModel.Recipe recipe);

    public static partial IEnumerable<ViewModel.Recipe> Map(IEnumerable<DBModel.Recipe> recipes);

    [MapProperty(nameof(DBModel.RecipeIngredient.IngredientName), nameof(ViewModel.RecipeIngredient.Name))]
    [MapProperty(nameof(DBModel.RecipeIngredient.UnitName), nameof(ViewModel.RecipeIngredient.Unit))]
    public static partial ViewModel.RecipeIngredient Map(DBModel.RecipeIngredient recipeIngredient);

    public static partial IEnumerable<DBModel.RecipeIngredient> Map(IEnumerable<ViewModel.RecipeIngredient> recipeIngredients);

    [MapProperty(nameof(ViewModel.RecipeIngredient.Name), nameof(DBModel.RecipeIngredient.IngredientName))]
    [MapProperty(nameof(ViewModel.RecipeIngredient.Unit), nameof(DBModel.RecipeIngredient.UnitName))]
    public static partial DBModel.RecipeIngredient Map(ViewModel.RecipeIngredient recipeIngredient);

    [MapProperty(nameof(DBModel.IngredientUnit.UnitType), nameof(ViewModel.IngredientUnit.Type))]
    public static partial ViewModel.IngredientUnit Map(DBModel.IngredientUnit ingredientUnit);

    public static partial IEnumerable<ViewModel.IngredientUnit> Map(IEnumerable<DBModel.IngredientUnit> ingredientUnit);

    [MapProperty(nameof(DBModel.GetRecipeIngredient.IngredientName), nameof(ViewModel.RecipeIngredient.Name))]
    [MapProperty(nameof(DBModel.GetRecipeIngredient.UnitName), nameof(ViewModel.RecipeIngredient.Unit))]
    [MapperIgnoreSource(nameof(DBModel.GetRecipeIngredient.UnitAbbreviation))]
    public static partial ViewModel.RecipeIngredient Map(DBModel.GetRecipeIngredient getRecipeIngredient);

    public static partial IEnumerable<ViewModel.RecipeIngredient> Map(IEnumerable<DBModel.GetRecipeIngredient> getRecipeIngredient);

    [MapperIgnoreTarget(nameof(ViewModel.FullRecipe.Ingredients))]
    public static partial ViewModel.FullRecipe? MapToFullRecipe(DBModel.Recipe? recipe);
}
