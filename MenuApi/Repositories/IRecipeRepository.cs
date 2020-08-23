using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.ViewModel;

namespace MenuApi.Repositories
{
    public interface IRecipeRepository
    {
        Task<Recipe> CreateRecipeAsync(NewRecipe newRecipe);

        Task<Recipe> GetRecipeAsync(int recipeId);

        Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId);

        Task<IEnumerable<Recipe>> GetRecipesAsync();

        Task<IEnumerable<Recipe>> SearchRecipesAsync(string q);

        Task<Recipe> UpdateRecipeAsync(Recipe newRecipe);
    }
}
