using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IRecipeService
{
    Task<RecipeId> CreateRecipeAsync(UpsertRecipe upsertRecipe);

    Task<RecipeDetail?> GetRecipeAsync(RecipeId recipeId);

    Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<RecipeListItem>> GetRecipesAsync();

    Task UpdateRecipeAsync(RecipeId recipeId, UpsertRecipe upsertRecipe);
}
