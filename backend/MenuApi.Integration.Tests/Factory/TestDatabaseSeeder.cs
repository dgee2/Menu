using Aspire.Hosting.Testing;
using MenuDB;
using MenuDB.Data;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Integration.Tests.Factory;

/// <summary>
/// Seeds rows directly into the shared integration-test database, bypassing the API.
/// Used to exercise ownership/scope behavior that can't be produced through the API alone,
/// since all HTTP calls in these tests authenticate as the same fixed Auth0 M2M identity.
/// </summary>
internal static class TestDatabaseSeeder
{
    public static async Task<Guid> AddMenuUserAsync(
        ApiTestFixture fixture,
        string authSubject,
        CancellationToken cancellationToken)
    {
        await using var db = await CreateDbContextAsync(fixture, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new MenuUserEntity
        {
            Id = Guid.CreateVersion7(),
            AuthSubject = authSubject,
            DisplayName = authSubject,
            CreatedAtUtc = now,
            LastSeenAtUtc = now,
        };

        db.MenuUsers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public static async Task<Guid> AddRecipeAsync(
        ApiTestFixture fixture,
        string title,
        Guid ownerUserId,
        string accessScopeName,
        CancellationToken cancellationToken)
    {
        await using var db = await CreateDbContextAsync(fixture, cancellationToken);

        // Resolved through the lookup table rather than hard-coded, so a seeded row that goes
        // missing shows up here as a failing test instead of a silently wrong scope.
        var accessScopeId = await db.RecipeAccessScopes
            .Where(s => s.Name == accessScopeName)
            .Select(s => s.Id)
            .SingleAsync(cancellationToken);

        var entity = new RecipeEntity
        {
            Id = Guid.CreateVersion7(),
            Title = title,
            OwnerUserId = ownerUserId,
            AccessScopeId = accessScopeId,
        };

        db.Recipes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public static async Task<int> CountStepsForRecipeAsync(
        ApiTestFixture fixture,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        await using var db = await CreateDbContextAsync(fixture, cancellationToken);

        return await db.RecipeSteps.CountAsync(s => s.RecipeId == recipeId, cancellationToken);
    }

    public static async Task<int> CountIngredientsForRecipeAsync(
        ApiTestFixture fixture,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        await using var db = await CreateDbContextAsync(fixture, cancellationToken);

        return await db.RecipeIngredients.CountAsync(i => i.RecipeId == recipeId, cancellationToken);
    }

    public static async Task SoftDeleteRecipeAsync(
        ApiTestFixture fixture,
        Guid recipeId,
        CancellationToken cancellationToken)
    {
        await using var db = await CreateDbContextAsync(fixture, cancellationToken);

        await db.Recipes
            .IgnoreQueryFilters()
            .Where(r => r.Id == recipeId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }

    private static async Task<MenuDbContext> CreateDbContextAsync(ApiTestFixture fixture, CancellationToken cancellationToken)
    {
        var connectionString = await fixture.app.GetConnectionStringAsync("menu", cancellationToken);

        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MenuDbContext(options);
    }
}
