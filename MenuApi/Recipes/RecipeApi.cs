using MenuApi.Services;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipe");

        group.WithTags("Recipes");

        group.MapGet("/", GetRecipes);

        group.MapGet("/{recipeId}", GetRecipe);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredients);

        group.MapPost("/", CreateRecipe);

        group.MapPut("{recipeId}", UpdateRecipe);

        return group;
    }

    public static async Task<IEnumerable<Recipe>> GetRecipes(IRecipeService recipeService)
    {
        return await recipeService.GetRecipesAsync();
    }

    public static async Task<FullRecipe> GetRecipe(IRecipeService recipeService, int recipeId)
    {
        return await recipeService.GetRecipeAsync(recipeId);
    }

    public static async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredients(IRecipeService recipeService, int recipeId)
    {
        return await recipeService.GetRecipeIngredientsAsync(recipeId);
    }

    public static async Task<FullRecipe> CreateRecipe(IRecipeService recipeService, NewRecipe newRecipe)
    {
        var recipeId = await recipeService.CreateRecipeAsync(newRecipe);
        return await recipeService.GetRecipeAsync(recipeId);
    }

    public static async Task<FullRecipe> UpdateRecipe(IRecipeService recipeService, int recipeId, NewRecipe newRecipe)
    {
        await recipeService.UpdateRecipeAsync(recipeId, newRecipe);
        return await recipeService.GetRecipeAsync(recipeId);
    }
}