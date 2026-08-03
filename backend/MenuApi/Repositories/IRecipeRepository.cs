using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IRecipeRepository
{
    Task<RecipeId> CreateRecipeAsync(DBModel.Recipe recipe);

    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take);

    Task UpdateRecipeAsync(RecipeId recipeId, DBModel.Recipe recipe);
}
