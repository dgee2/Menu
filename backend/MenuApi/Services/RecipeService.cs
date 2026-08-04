using MenuDB;
using MenuApi.Exceptions;
using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Services;

public class RecipeService(IRecipeRepository recipeRepository, IRecipeStepRepository recipeStepRepository, MenuDbContext db) : IRecipeService
{
    public async Task<IEnumerable<RecipeListItem>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take)
    {
        var recipes = await recipeRepository.GetRecipesAsync(scope, callerId, take).ConfigureAwait(false);
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

    public async Task<RecipeId> CreateRecipeAsync(UpsertRecipe upsertRecipe, MenuUserId callerId)
    {
        ArgumentNullException.ThrowIfNull(upsertRecipe);

        var recipe = ViewModelMapper.Map(upsertRecipe) with { OwnerUserId = callerId };

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            var recipeId = await recipeRepository.CreateRecipeAsync(recipe).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Ingredients)).ConfigureAwait(false);
            await recipeStepRepository.UpsertStepCollectionAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Steps)).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
            return recipeId;
        }).ConfigureAwait(false);
    }

    public async Task<bool> UpdateRecipeAsync(RecipeId recipeId, UpsertRecipe upsertRecipe, MenuUserId callerId)
    {
        ArgumentNullException.ThrowIfNull(upsertRecipe);

        var existing = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        if (existing.OwnerUserId != callerId)
        {
            throw new ForbiddenAccessException($"You do not own recipe {recipeId}.");
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            await recipeRepository.UpdateRecipeAsync(recipeId, ViewModelMapper.Map(upsertRecipe)).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Ingredients)).ConfigureAwait(false);
            await recipeStepRepository.UpsertStepCollectionAsync(recipeId, ViewModelMapper.Map(upsertRecipe.Steps)).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);

        return true;
    }
}
