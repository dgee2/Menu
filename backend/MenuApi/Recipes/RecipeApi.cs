using MenuApi.Middleware;
using MenuApi.Services;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    private const int DefaultTake = 50;
    private const int MaxTake = 200;

    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipe");

        group.WithTags("Recipes");

        group.MapGet("/", GetRecipesAsync)
            .Produces<IEnumerable<RecipeListItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{recipeId}", GetRecipeAsync)
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredientsAsync)
            .Produces<IEnumerable<RecipeIngredientItem>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateRecipeAsync)
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("{recipeId}", UpdateRecipeAsync)
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("{recipeId}", DeleteRecipeAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    public static async Task<IResult> GetRecipesAsync(
        IRecipeService recipeService,
        HttpContext httpContext,
        string? scope,
        int? take)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId callerId)
        {
            return Results.Unauthorized();
        }

        if (!TryParseScope(scope, out var recipeListScope))
        {
            var detail = string.IsNullOrEmpty(scope)
                ? "Missing scope. Expected 'mine' or 'authenticated'."
                : $"Unknown scope '{scope}'. Expected 'mine' or 'authenticated'.";

            return Results.Problem(
                detail: detail,
                statusCode: StatusCodes.Status400BadRequest);
        }

        var boundedTake = Math.Clamp(take ?? DefaultTake, 1, MaxTake);

        var recipes = await recipeService.GetRecipesAsync(recipeListScope, callerId, boundedTake);
        return Results.Ok(recipes);
    }

    private static bool TryParseScope(string? scope, out RecipeListScope recipeListScope)
    {
        switch (scope?.ToLowerInvariant())
        {
            case "mine":
                recipeListScope = RecipeListScope.Mine;
                return true;
            case "authenticated":
                recipeListScope = RecipeListScope.Authenticated;
                return true;
            default:
                recipeListScope = default;
                return false;
        }
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

    public static async Task<IResult> CreateRecipeAsync(IRecipeService recipeService, HttpContext httpContext, UpsertRecipe upsertRecipe)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId callerId)
        {
            return Results.Unauthorized();
        }

        var recipeId = await recipeService.CreateRecipeAsync(upsertRecipe, callerId);
        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return Results.Ok(recipe ?? throw new InvalidOperationException("Recipe creation failed"));
    }

    public static async Task<IResult> UpdateRecipeAsync(IRecipeService recipeService, HttpContext httpContext, RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId callerId)
        {
            return Results.Unauthorized();
        }

        var updated = await recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId);
        if (!updated)
        {
            return Results.Problem(
                detail: $"Recipe with ID {recipeId} was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        var recipe = await recipeService.GetRecipeAsync(recipeId);
        return Results.Ok(recipe ?? throw new InvalidOperationException($"Failed to retrieve the updated recipe with ID {recipeId} after the update operation."));
    }

    public static async Task<IResult> DeleteRecipeAsync(IRecipeService recipeService, HttpContext httpContext, RecipeId recipeId)
    {
        if (httpContext.Items[MenuUserHttpContextKeys.MenuUserId] is not MenuUserId callerId)
        {
            return Results.Unauthorized();
        }

        var deleted = await recipeService.DeleteRecipeAsync(recipeId, callerId);
        if (!deleted)
        {
            return Results.Problem(
                detail: $"Recipe with ID {recipeId} was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.NoContent();
    }
}
