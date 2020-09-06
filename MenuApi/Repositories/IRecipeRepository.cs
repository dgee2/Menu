using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenuApi.Repositories
{
    public interface IRecipeRepository
    {
        Task<int> CreateRecipeAsync(string name);

        Task<DBModel.Recipe> GetRecipeAsync(int recipeId);

        Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients);

        Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId);

        Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

        Task<IEnumerable<DBModel.Recipe>> SearchRecipesAsync(string q);

        Task<DBModel.Recipe> UpdateRecipeAsync(DBModel.Recipe newRecipe);
    }
}
