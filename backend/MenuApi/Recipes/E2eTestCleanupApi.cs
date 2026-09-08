using MenuApi.Middleware;
using MenuApi.ValueObjects;
using MenuDB;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Recipes;

/// <summary>
/// Maps the physical cleanup endpoint used only by the Playwright suite. It is deliberately not
/// part of the normal API surface: <see cref="Program"/> maps it only when the AppHost was started
/// by the E2E test configuration.
/// </summary>
public static class E2eTestCleanupApi
{
    public static void MapE2eTestCleanup(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/e2e-test/recipe")
            .WithTags("E2E test cleanup")
            .AddEndpointFilter<RequireCallerFilter>();

        group.MapGet("/status", () => Results.NoContent())
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{recipeId}", HardDeleteRecipeAsync)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> HardDeleteRecipeAsync(
        MenuDbContext db,
        CallerId caller,
        RecipeId recipeId)
    {
        var deleted = await db.Recipes
            .IgnoreQueryFilters()
            .Where(recipe => recipe.Id == recipeId.Value && recipe.OwnerUserId == caller.Value.Value)
            .ExecuteDeleteAsync()
            .ConfigureAwait(false);

        return deleted == 0
            ? Results.NotFound()
            : Results.NoContent();
    }
}
