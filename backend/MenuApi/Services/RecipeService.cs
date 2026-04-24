using MenuDB;
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

        var ingredients = ViewModelMapper.Map(newRecipe.Ingredients)
            .GroupBy(i => new { IngredientName = i.IngredientName.Value, UnitName = i.UnitName.Value })
            .Select(g => g.First())
            .ToList();

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

        var ingredients = ViewModelMapper.Map(newRecipe.Ingredients)
            .GroupBy(i => new { IngredientName = i.IngredientName.Value, UnitName = i.UnitName.Value })
            .Select(g => g.First())
            .ToList();

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tran = await db.Database.BeginTransactionAsync().ConfigureAwait(false);
            await recipeRepository.UpdateRecipeAsync(recipeId, newRecipe.Name).ConfigureAwait(false);
            await recipeRepository.UpsertRecipeIngredientsAsync(recipeId, ingredients).ConfigureAwait(false);
            await tran.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
