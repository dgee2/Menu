using AutoMapper;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.StrongIds;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class RecipeService(IRecipeRepository recipeRepository, IMapper mapper, ITransactionFactory transactionFactory) : IRecipeService
{
    public async Task<IEnumerable<Recipe>> GetRecipesAsync()
    {
        var recipes = await recipeRepository.GetRecipesAsync().ConfigureAwait(false);
        return recipes.Select(mapper.Map<Recipe>);
    }

    public async Task<FullRecipe> GetRecipeAsync(RecipeId recipeId)
    {
        var dbRecipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
        var dbIngredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

        var recipe = mapper.Map<FullRecipe>(dbRecipe);
        recipe.Ingredients = dbIngredients.Select(mapper.Map<RecipeIngredient>);

        return recipe;
    }

    public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId)
    {
        var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
        return ingredients.Select(mapper.Map<RecipeIngredient>);
    }

    public async Task<RecipeId> CreateRecipeAsync(NewRecipe newRecipe)
    {
        ArgumentNullException.ThrowIfNull(newRecipe);

        var ingredients = newRecipe.Ingredients.Select(mapper.Map<DBModel.RecipeIngredient>);

        using var tran = transactionFactory.BeginTransaction();
        var recipeId = await recipeRepository.CreateRecipeAsync(newRecipe.Name, tran).ConfigureAwait(false);

        await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);

        tran.Commit();
        return recipeId;
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, NewRecipe newRecipe)
    {
        ArgumentNullException.ThrowIfNull(newRecipe);

        var ingredients = newRecipe.Ingredients.Select(mapper.Map<DBModel.RecipeIngredient>);

        using var tran = transactionFactory.BeginTransaction();
        await recipeRepository.UpdateRecipeAsync(recipeId, newRecipe.Name, tran).ConfigureAwait(false);

        await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients, tran).ConfigureAwait(false);
        tran.Commit();
    }
}
