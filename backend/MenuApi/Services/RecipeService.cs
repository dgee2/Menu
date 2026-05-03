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

        if (await recipeRepository.RecipeNameExistsAsync(newRecipe.Name).ConfigureAwait(false))
        {
            throw new ConflictException($"Recipe '{newRecipe.Name.Value}' already exists.");
        }

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

        if (await recipeRepository.RecipeNameExistsAsync(newRecipe.Name, recipeId).ConfigureAwait(false))
        {
            throw new ConflictException($"Recipe '{newRecipe.Name.Value}' already exists.");
        }

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

        var mappedIngredients = ViewModelMapper.Map(recipeIngredients).ToList();
        var conflictingDuplicates = mappedIngredients
            .GroupBy(ingredient => new
            {
                IngredientName = ingredient.IngredientName.Value,
                UnitName = ingredient.UnitName.Value,
            })
            .Where(group => group.Select(ingredient => ingredient.Amount.Value).Distinct().Count() > 1)
            .Select(group =>
                $"Ingredient '{group.Key.IngredientName}' with unit '{group.Key.UnitName}' is specified multiple times with different amounts.")
            .ToArray();

        if (conflictingDuplicates.Length != 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["ingredients"] = conflictingDuplicates,
            });
        }

        return [.. mappedIngredients.Distinct()];
    }
}
