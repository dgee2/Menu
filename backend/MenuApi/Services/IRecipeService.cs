using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IRecipeService
{
    Task<RecipeId> CreateRecipeAsync(UpsertRecipe upsertRecipe, MenuUserId callerId);

    Task<RecipeDetail?> GetRecipeAsync(RecipeId recipeId);

    Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<RecipeListItem>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take);

    Task UpdateRecipeAsync(RecipeId recipeId, UpsertRecipe upsertRecipe);
}
