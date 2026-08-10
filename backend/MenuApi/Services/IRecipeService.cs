using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IRecipeService
{
    Task<RecipeId> CreateRecipeAsync(UpsertRecipe upsertRecipe, MenuUserId callerId);

    /// <summary>
    /// Returns the recipe if <paramref name="callerId"/> may read it, otherwise <see langword="null"/>
    /// - callers turn that into a 404 rather than a 403, so a private recipe's existence does not leak.
    /// </summary>
    Task<RecipeDetail?> GetRecipeAsync(RecipeId recipeId, MenuUserId callerId);

    Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(RecipeId recipeId, MenuUserId callerId);

    Task<IEnumerable<RecipeListItem>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take);

    Task<bool> UpdateRecipeAsync(RecipeId recipeId, UpsertRecipe upsertRecipe, MenuUserId callerId);

    Task<bool> DeleteRecipeAsync(RecipeId recipeId, MenuUserId callerId);
}
