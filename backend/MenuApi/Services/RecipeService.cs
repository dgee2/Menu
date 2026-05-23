using MenuDB;
using MenuApi.Exceptions;
using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Services;

public class RecipeService(IRecipeRepository recipeRepository, MenuDbContext db) : IRecipeService
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

        if (recipe is not null)
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

        var ingredients = NormalizeRecipeIngredients(newRecipe.Ingredients);

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            var recipeId = await recipeRepository.CreateRecipeAsync(newRecipe.Name).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
            return recipeId;
        }).ConfigureAwait(false);
    }

    public async Task UpdateRecipeAsync(RecipeId recipeId, NewRecipe newRecipe)
    {
        ArgumentNullException.ThrowIfNull(newRecipe);

        var ingredients = NormalizeRecipeIngredients(newRecipe.Ingredients);

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            await recipeRepository.UpdateRecipeAsync(recipeId, newRecipe.Name).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private static IReadOnlyList<DBModel.RecipeIngredient> NormalizeRecipeIngredients(IEnumerable<ViewModel.RecipeIngredient> recipeIngredients)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredients);

        var deduped = ViewModelMapper.Map(recipeIngredients).Distinct().ToList();

        // After exact-duplicate removal, any (IngredientName, UnitName) group with more than one
        // entry represents the same ingredient+unit with conflicting amounts — a business conflict.
        var conflicts = deduped
            .GroupBy(i => (i.IngredientName, i.UnitName))
            .Where(g => g.Count() > 1)
            .Select(g => $"'{g.Key.IngredientName.Value}' with unit '{g.Key.UnitName.Value}'")
            .ToList();

        if (conflicts.Count > 0)
        {
            throw new BusinessValidationException(
                $"The following ingredients appear more than once with conflicting amounts: {string.Join(", ", conflicts)}.");
        }

        return deduped;
    }
}
