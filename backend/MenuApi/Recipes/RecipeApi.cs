using MenuApi.Services;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipe");

        group.WithTags("Recipes");

        group.MapGet("/", GetRecipesAsync)
            .Produces<IEnumerable<RecipeListItem>>(StatusCodes.Status200OK);

        group.MapGet("/{recipeId}", GetRecipeAsync)
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredientsAsync)
            .Produces<IEnumerable<RecipeIngredientItem>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateRecipeAsync)
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("{recipeId}", UpdateRecipeAsync)
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }

    public static async Task<IEnumerable<RecipeListItem>> GetRecipesAsync(IRecipeService recipeService)
    {
        return await recipeService.GetRecipesAsync();
    }

    public static async Task<IResult> GetRecipeAsync(IRecipeService recipeService, RecipeId recipeId)
    {
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return recipe is not null
            ? Results.Ok(recipe)
            : Results.Problem(
                detail: $"Recipe with ID {recipeId} was not found.",
                statusCode: StatusCodes.Status404NotFound);
    }

    public static async Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(IRecipeService recipeService, RecipeId recipeId)
    {
        return await recipeService.GetRecipeIngredientsAsync(recipeId);
    }

    public static async Task<RecipeDetail> CreateRecipeAsync(IRecipeService recipeService, UpsertRecipe upsertRecipe)
    {
        var recipeId = await recipeService.CreateRecipeAsync(upsertRecipe);
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return recipe ?? throw new InvalidOperationException("Recipe creation failed");
    }

    public static async Task<RecipeDetail> UpdateRecipeAsync(IRecipeService recipeService, RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        await recipeService.UpdateRecipeAsync(recipeId, upsertRecipe);
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return recipe ?? throw new InvalidOperationException($"Failed to retrieve the updated recipe with ID {recipeId} after the update operation.");
    }
}
