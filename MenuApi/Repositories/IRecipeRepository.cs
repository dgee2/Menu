using MenuApi.StrongIds;
using System.Data;

namespace MenuApi.Repositories;

public interface IRecipeRepository
{
    Task<RecipeId> CreateRecipeAsync(string name);

    Task<RecipeId> CreateRecipeAsync(string name, IDbTransaction? transaction);

    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId);

    Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId, IDbTransaction? transaction);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

    Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients, IDbTransaction? transaction);

    Task<IEnumerable<DBModel.GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId);

    Task<IEnumerable<DBModel.GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId, IDbTransaction? transaction);

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

    Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(IDbTransaction? transaction);

    Task UpdateRecipeAsync(RecipeId recipeId, string name);

    Task UpdateRecipeAsync(RecipeId recipeId, string name, IDbTransaction? transaction);
}
