using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IRecipeRepository
{
    Task<RecipeId> CreateRecipeAsync(DBModel.Recipe recipe);

    /// <summary>
    /// Fetches a recipe without applying any access filter. For the write path, which needs to tell
    /// "does not exist" (404) from "exists but is not yours" (403). Read endpoints must use
    /// <see cref="GetReadableRecipeAsync"/>.
    /// </summary>
    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId);

    /// <summary>
    /// Fetches a recipe only if <paramref name="callerId"/> is allowed to read it, so an unreadable
    /// recipe is indistinguishable from a missing one.
    /// </summary>
    Task<DBModel.Recipe?> GetReadableRecipeAsync(RecipeId recipeId, MenuUserId callerId);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId, MenuUserId callerId);

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take);

    Task UpdateRecipeAsync(RecipeId recipeId, DBModel.Recipe recipe);

    Task DeleteRecipeAsync(RecipeId recipeId);
}
