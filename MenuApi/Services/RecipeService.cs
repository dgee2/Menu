using MenuApi.Factory;
using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class RecipeService(IRecipeRepository recipeRepository, ITransactionFactory transactionFactory) : IRecipeService
{
    public async Task<IEnumerable<Recipe>> GetRecipesAsync()
    {
        var recipes = await recipeRepository.GetRecipesAsync().ConfigureAwait(false);
        return ViewModelMapper.Map(recipes);
    }

    public async Task<FullRecipe?> GetRecipeAsync(RecipeId recipeId)
    {
        var dbRecipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);

        var recipe = ViewModelMapper.MapToFullRecipe(dbRecipe);

        if ((recipe is not null))
        {
            recipe.Ingredients = await GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
        }

        return recipe;
    }

    public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId)
    {
        var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
        return ViewModelMapper.Map(ingredients);
    }

    public async Task<RecipeId> CreateRecipeAsync(NewRecipe newRecipe)
    {
        ArgumentNullException.ThrowIfNull(newRecipe);

        var ingredients = ViewModelMapper.Map(newRecipe.Ingredients);

        using var tran = transactionFactory.BeginTransaction();
        var recipeId = await recipeRepository.CreateRecipeAsync(newRecipe.Name, tran).ConfigureAwait(false);

        await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);

        tran.Commit();
        return recipeId;
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, NewRecipe newRecipe)
    {
        ArgumentNullException.ThrowIfNull(newRecipe);

        var ingredients = ViewModelMapper.Map(newRecipe.Ingredients);

        using var tran = transactionFactory.BeginTransaction();
        await recipeRepository.UpdateRecipeAsync(recipeId, newRecipe.Name, tran).ConfigureAwait(false);

        await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);
        tran.Commit();
    }
}
