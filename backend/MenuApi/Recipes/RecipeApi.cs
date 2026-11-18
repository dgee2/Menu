using MenuApi.Middleware;
using MenuApi.Services;
using MenuApi.Validation;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Recipes;

public static class RecipeApi
{
    internal const int DefaultTake = 50;
    internal const string GetRecipesOperationId = "getRecipes";
    private const int MaxTake = 200;
    private const string JsonContentType = "application/json";

    public static RouteGroupBuilder MapRecipes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/recipe");

        group.WithTags("Recipes");

        // Every recipe endpoint reads or writes on behalf of a specific Menu user, so the caller
        // guard belongs on the group rather than being restated in each handler.
        group.AddEndpointFilter<RequireCallerFilter>();

        group.MapGet("/", GetRecipesAsync)
            .WithName(GetRecipesOperationId)
            .Produces<IEnumerable<RecipeListItem>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        group.MapGet("/{recipeId}", GetRecipeAsync)
            .WithName("getRecipe")
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredientsAsync)
            .WithName("getRecipeIngredients")
            .Produces<IEnumerable<RecipeIngredientItem>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateRecipeAsync)
            .WithName("createRecipe")
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict, JsonContentType);

        group.MapPut("{recipeId}", UpdateRecipeAsync)
            .WithName("updateRecipe")
            .AddEndpointFilter<ValidationFilter<UpsertRecipe>>()
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden, JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict, JsonContentType);

        group.MapDelete("{recipeId}", DeleteRecipeAsync)
            .WithName("deleteRecipe")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden, JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{recipeId}/restore", RestoreRecipeAsync)
            .WithName("restoreRecipe")
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden, JsonContentType)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict, JsonContentType);

        return group;
    }

    public static async Task<IResult> GetRecipesAsync(
        IRecipeService recipeService,
        CallerId caller,
        string? scope,
        int? take,
        string? query = null)
    {
        if (!RecipeListScopeParser.TryParse(scope, out var recipeListScope))
        {
            var detail = string.IsNullOrEmpty(scope)
                ? $"Missing scope. Expected one of: {string.Join(", ", RecipeListScopeParser.AllValues)}."
                : $"Unknown scope '{scope}'. Expected one of: {string.Join(", ", RecipeListScopeParser.AllValues)}.";

            // Keyed on the offending parameter so a client can surface it against the right input,
            // rather than a bare problem the caller has to read prose out of.
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["scope"] = [detail],
            });
        }

        var boundedTake = Math.Clamp(take ?? DefaultTake, 1, MaxTake);

        var recipes = await recipeService.GetRecipesAsync(recipeListScope, caller.Value, boundedTake, query);
        return Results.Ok(recipes);
    }

    public static async Task<IResult> GetRecipeAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId)
    {
        var recipe = await recipeService.GetRecipeAsync(recipeId, caller.Value, recordView: true);
        return recipe is not null
            ? Results.Ok(recipe)
            : RecipeNotFound(recipeId);
    }

    public static async Task<IEnumerable<RecipeIngredientItem>> GetRecipeIngredientsAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId)
    {
        return await recipeService.GetRecipeIngredientsAsync(recipeId, caller.Value);
    }

    public static async Task<IResult> CreateRecipeAsync(IRecipeService recipeService, CallerId caller, UpsertRecipe upsertRecipe)
    {
        var recipeId = await recipeService.CreateRecipeAsync(upsertRecipe, caller.Value);
        var recipe = await recipeService.GetRecipeAsync(recipeId, caller.Value);
        return Results.Ok(recipe ?? throw new InvalidOperationException("Recipe creation failed"));
    }

    public static async Task<IResult> UpdateRecipeAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        var updated = await recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, caller.Value);
        if (!updated)
        {
            return RecipeNotFound(recipeId);
        }

        var recipe = await recipeService.GetRecipeAsync(recipeId, caller.Value);
        return Results.Ok(recipe ?? throw new InvalidOperationException($"Failed to retrieve the updated recipe with ID {recipeId} after the update operation."));
    }

    public static async Task<IResult> DeleteRecipeAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId)
    {
        var deleted = await recipeService.DeleteRecipeAsync(recipeId, caller.Value);
        if (!deleted)
        {
            return RecipeNotFound(recipeId);
        }

        return Results.NoContent();
    }

    public static async Task<IResult> RestoreRecipeAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId)
    {
        var restored = await recipeService.RestoreRecipeAsync(recipeId, caller.Value);
        if (!restored)
        {
            return RecipeNotFound(recipeId);
        }

        var recipe = await recipeService.GetRecipeAsync(recipeId, caller.Value);
        return Results.Ok(recipe ?? throw new InvalidOperationException($"Failed to retrieve the restored recipe with ID {recipeId}."));
    }

    /// <summary>
    /// The response for a recipe the caller cannot see, whether or not it exists. Deliberately a 404
    /// rather than a 403 - the real reason is logged server-side, not handed to the caller, so recipe
    /// ids cannot be probed for the existence of other people's private recipes.
    /// </summary>
    private static IResult RecipeNotFound(RecipeId recipeId) => Results.Problem(
        detail: $"Recipe with ID {recipeId} was not found.",
        statusCode: StatusCodes.Status404NotFound);
}
