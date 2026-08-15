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

        // Every recipe endpoint reads or writes on behalf of a specific Menu user, so the caller
        // guard belongs on the group rather than being restated in each handler.
        group.AddEndpointFilter<RequireCallerFilter>();

        group.MapGet("/", GetRecipesAsync)
            .Produces<IEnumerable<RecipeListItem>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{recipeId}", GetRecipeAsync)
            .Produces<RecipeDetail>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{recipeId}/ingredient", GetRecipeIngredientsAsync)
            .Produces<IEnumerable<RecipeIngredientItem>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

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
        CallerId caller,
        string? scope,
        int? take)
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

        var recipes = await recipeService.GetRecipesAsync(recipeListScope, caller.Value, boundedTake);
        return Results.Ok(recipes);
    }

    public static async Task<IResult> GetRecipeAsync(IRecipeService recipeService, CallerId caller, RecipeId recipeId)
    {
        var recipe = await recipeService.GetRecipeAsync(recipeId, caller.Value);
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

    /// <summary>
    /// The response for a recipe the caller cannot see, whether or not it exists. Deliberately a 404
    /// rather than a 403 - the real reason is logged server-side, not handed to the caller, so recipe
    /// ids cannot be probed for the existence of other people's private recipes.
    /// </summary>
    private static IResult RecipeNotFound(RecipeId recipeId) => Results.Problem(
        detail: $"Recipe with ID {recipeId} was not found.",
        statusCode: StatusCodes.Status404NotFound);
}
