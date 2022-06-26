using MenuApi.ViewModel;

namespace MenuApi.Services
{
    public interface IRecipeService
    {
        Task<int> CreateRecipeAsync(NewRecipe newRecipe);

        Task<FullRecipe> GetRecipeAsync(int recipeId);

        Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId);

        Task<IEnumerable<Recipe>> GetRecipesAsync();

        Task UpdateRecipeAsync(int recipeId, NewRecipe newRecipe);
    }
}
