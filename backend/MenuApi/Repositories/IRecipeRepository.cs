using MenuApi.ValueObjects;

namespace MenuApi.Repositories;

public interface IRecipeRepository
{
    Task<RecipeId> CreateRecipeAsync(RecipeTitle title);

    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    Task<IEnumerable<DBModel.GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

    Task UpdateRecipeAsync(RecipeId recipeId, RecipeTitle title);
}
