using MenuApi.Services;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipes");

        group.WithTags("Recipes");

        group.MapGet("/", async (IRecipeService recipeService) =>
        {
            return await recipeService.GetRecipesAsync();
        });

        group.MapGet("/{recipeId}", async (IRecipeService recipeService, int recipeId) =>
        {
            return await recipeService.GetRecipeAsync(recipeId);
        });

        group.MapGet("/{recipeId}/Ingredient", async (IRecipeService recipeService, int recipeId) =>
        {
            return await recipeService.GetRecipeIngredientsAsync(recipeId);
        });

        group.MapPost("/", async (IRecipeService recipeService, NewRecipe newRecipe) =>
        {
            var recipeId = await recipeService.CreateRecipeAsync(newRecipe);
            return await recipeService.GetRecipeAsync(recipeId);
        });

        group.MapPut("{recipeId}", async (IRecipeService recipeService, int recipeId, NewRecipe newRecipe) =>
        {
            await recipeService.UpdateRecipeAsync(recipeId, newRecipe);
            return await recipeService.GetRecipeAsync(recipeId);
        });

        return group;
    }
}