using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipe");

        group.WithTags("Recipes");

        group.MapGet("/", GetRecipesAsync);

        group.MapGet("/{recipeId}", GetRecipeAsync);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredientsAsync);

        group.MapPost("/", CreateRecipeAsync);

        group.MapPut("{recipeId}", UpdateRecipeAsync);

        return group;
    }

    public static async Task<IEnumerable<Recipe>> GetRecipesAsync(IRecipeService recipeService)
    {
        return await recipeService.GetRecipesAsync();
    }

    public static async Task<FullRecipe?> GetRecipeAsync(IRecipeService recipeService, RecipeId recipeId)
    {
        return await recipeService.GetRecipeAsync(recipeId);
    }

    public static async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(IRecipeService recipeService, RecipeId recipeId)
    {
        return await recipeService.GetRecipeIngredientsAsync(recipeId);
    }

    public static async Task<FullRecipe> CreateRecipeAsync(IRecipeService recipeService, NewRecipe newRecipe)
    {
        var recipeId = await recipeService.CreateRecipeAsync(newRecipe);
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return recipe ?? throw new InvalidOperationException("Recipe creation failed");
    }

    public static async Task<FullRecipe> UpdateRecipeAsync(IRecipeService recipeService, RecipeId recipeId, NewRecipe newRecipe)
    {
        await recipeService.UpdateRecipeAsync(recipeId, newRecipe);
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return recipe ?? throw new InvalidOperationException($"Failed to retrieve the updated recipe with ID {recipeId} after the update operation.");
    }
}