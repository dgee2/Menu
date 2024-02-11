using MenuApi.StrongIds;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IRecipeService
{
    Task<RecipeId> CreateRecipeAsync(NewRecipe newRecipe);

    Task<FullRecipe> GetRecipeAsync(RecipeId recipeId);

    Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<Recipe>> GetRecipesAsync();

    Task UpdateRecipeAsync(RecipeId recipeId, NewRecipe newRecipe);
}
