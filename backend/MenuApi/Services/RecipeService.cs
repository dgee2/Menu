using MenuDB;
using MenuApi.Authorization;
using MenuApi.Exceptions;
using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Services;

public class RecipeService(
    IRecipeRepository recipeRepository,
    IRecipeStepRepository recipeStepRepository,
    MenuDbContext db,
    ILogger<RecipeService> logger) : IRecipeService
{
    public async Task<IEnumerable<RecipeListItem>> GetRecipesAsync(RecipeListScope scope, MenuUserId callerId, int take)
    {
        var recipes = await recipeRepository.GetRecipesAsync(scope, callerId, take).ConfigureAwait(false);
        return ViewModelMapper.Map(recipes);
    }

    public async Task<RecipeDetail?> GetRecipeAsync(RecipeId recipeId, MenuUserId callerId)
    {
        var dbRecipe = await recipeRepository.GetReadableRecipeAsync(recipeId, callerId).ConfigureAwait(false);

        if (dbRecipe is null)
        {
            await LogWhyNotReadableAsync(recipeId, callerId).ConfigureAwait(false);
            return null;
        }

        var recipe = ViewModelMapper.MapToRecipeDetail(dbRecipe);

        if (recipe is not null)
        {
            recipe.CanEdit = RecipeAccessRules.CanEdit(dbRecipe, callerId);
            recipe.CanDelete = RecipeAccessRules.CanDelete(dbRecipe, callerId);

            recipe.Ingredients = await GetRecipeIngredientsAsync(recipeId, callerId).ConfigureAwait(false);

            var steps = await recipeStepRepository.GetStepsByRecipeIdAsync(recipeId).ConfigureAwait(false);
            recipe.Steps = ViewModelMapper.Map(steps);
        }

        return recipe;
    }

    public async Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(RecipeId recipeId, MenuUserId callerId)
    {
        var ingredients = await recipeRepository.GetRecipeIngredientsAsync(recipeId, callerId).ConfigureAwait(false);
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

        if (await LoadOwnedRecipeAsync(recipeId, callerId).ConfigureAwait(false) is null)
        {
            return false;
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

    public async Task<bool> DeleteRecipeAsync(RecipeId recipeId, MenuUserId callerId)
    {
        if (await LoadOwnedRecipeAsync(recipeId, callerId).ConfigureAwait(false) is null)
        {
            return false;
        }

        await recipeRepository.DeleteRecipeAsync(recipeId).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// The caller sees an undifferentiated 404, so record server-side which of the two it actually
    /// was. Only runs on the miss path, where an extra round trip costs nothing worth saving.
    /// </summary>
    private async Task LogWhyNotReadableAsync(RecipeId recipeId, MenuUserId callerId)
    {
        var exists = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false) is not null;

        if (exists)
        {
            logger.LogInformation(
                "Recipe {RecipeId} was not returned to user {CallerId}: it exists but the caller may not read it. Responding 404.",
                recipeId.Value,
                callerId.Value);
        }
        else
        {
            logger.LogInformation("Recipe {RecipeId} requested by user {CallerId} does not exist.", recipeId.Value, callerId.Value);
        }
    }

    /// <summary>
    /// Loads a recipe for a write operation, distinguishing "does not exist" from "not yours".
    /// </summary>
    /// <returns>The recipe, or <see langword="null"/> when no recipe has that id.</returns>
    /// <exception cref="ForbiddenAccessException">The recipe exists but <paramref name="callerId"/> may not modify it.</exception>
    private async Task<DBModel.Recipe?> LoadOwnedRecipeAsync(RecipeId recipeId, MenuUserId callerId)
    {
        var existing = await recipeRepository.GetRecipeAsync(recipeId).ConfigureAwait(false);
        if (existing is null)
        {
            return null;
        }

        if (!RecipeAccessRules.CanEdit(existing, callerId))
        {
            throw new ForbiddenAccessException($"You do not own recipe {recipeId}.");
        }

        return existing;
    }
}
