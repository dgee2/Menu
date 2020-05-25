using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.ViewModel;

namespace MenuApi.Repositories
{
    public interface IRecipeRepository
    {
        Task<Recipe> CreateRecipeAsync(NewRecipe newRecipe);

        IAsyncEnumerable<Recipe> GetRecipesAsync();

        Task<IEnumerable<Recipe>> SearchRecipesAsync(string q);

        Task<Recipe> UpdateRecipeAsync(Recipe newRecipe);
    }
}
