using MenuDB;
using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Services;

public class RecipeService(IRecipeRepository recipeRepository, IRecipeStepRepository recipeStepRepository, MenuDbContext db) : IRecipeService
{
    public async Task<IEnumerable<RecipeListItem>> GetRecipesAsync()
    {
        var recipes = await recipeRepository.GetRecipesAsync().ConfigureAwait(false);
        return ViewModelMapper.Map(recipes);
    }

    public async Task<RecipeDetail?> GetRecipeAsync(RecipeId recipeId)
    {
        var dbRecipe = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);

        var recipe = ViewModelMapper.MapToRecipeDetail(dbRecipe);

        if (recipe is not null)
        {
            recipe.Ingredients = await GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

            var steps = await recipeStepRepository.GetStepsByRecipeIdAsync(recipeId).ConfigureAwait(false);
            recipe.Steps = ViewModelMapper.Map(steps);
        }

        return recipe;
    }

    public async Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(RecipeId recipeId)
    {
        var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
        return ViewModelMapper.Map(ingredients);
    }

    public async Task<RecipeId> CreateRecipeAsync(UpsertRecipe upsertRecipe)
    {
        ArgumentNullException.ThrowIfNull(upsertRecipe);

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            var recipeId = await recipeRepository.CreateRecipeAsync(ViewModelMapper.Map(upsertRecipe)).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Ingredients)).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
            return recipeId;
        }).ConfigureAwait(false);
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        ArgumentNullException.ThrowIfNull(upsertRecipe);

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            await recipeRepository.UpdateRecipeAsync(recipeId, ViewModelMapper.Map(upsertRecipe)).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Ingredients)).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
