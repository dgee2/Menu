using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenuApi.Repositories
{
    public interface IRecipeRepository
    {
        Task<DBModel.Recipe> CreateRecipeAsync(string name, IEnumerable<ViewModel.RecipeIngredient> ingredients);

        Task<DBModel.Recipe> GetRecipeAsync(int recipeId);

        Task<IEnumerable<DBModel.RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId);

        Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync();

        Task<IEnumerable<DBModel.Recipe>> SearchRecipesAsync(string q);

        Task<DBModel.Recipe> UpdateRecipeAsync(DBModel.Recipe newRecipe);
    }
}
