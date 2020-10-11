using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MenuApi.Repositories
{
    public interface IRecipeRepository
    {
        Task<int> CreateRecipeAsync(string name);

        Task<int> CreateRecipeAsync(string name, IDbTransaction? transaction);

        Task<DBModel.Recipe> GetRecipeAsync(int recipeId);

        Task<DBModel.Recipe> GetRecipeAsync(int recipeId, IDbTransaction? transaction);

        Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

        Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients, IDbTransaction? transaction);

        Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId);

        Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId, IDbTransaction? transaction);

        Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

        Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(IDbTransaction? transaction);

        Task<IEnumerable<DBModel.Recipe>> SearchRecipesAsync(string q);

        Task UpdateRecipeAsync(int recipeId, string name);

        Task UpdateRecipeAsync(int recipeId, string name, IDbTransaction? transaction);
    }
}
