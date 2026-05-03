using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IRecipeRepository
{
    Task<RecipeId> CreateRecipeAsync(RecipeName name);

    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    Task<IEnumerable<DBModel.GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

    Task<bool> RecipeNameExistsAsync(RecipeName name);

    Task<bool> RecipeNameExistsAsync(RecipeName name, RecipeId excludedRecipeId);

    Task UpdateRecipeAsync(RecipeId recipeId, RecipeName name);
}
