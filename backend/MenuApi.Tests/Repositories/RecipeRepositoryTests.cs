using AwesomeAssertions;
using MenuDB;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

// UpdateRecipeAsync uses ExecuteUpdateAsync, which is not supported by the EF Core
// InMemory provider (it requires a relational provider). That code path is covered by
// the RecipeRepository source review rather than a unit test here, to avoid pulling in
// an additional relational test provider (e.g. SQLite) purely for test infrastructure.
public class RecipeRepositoryTests
{
    [Fact]
    public async Task CreateRecipeAsync_Sets_Audit_Timestamps()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        var sut = new RecipeRepository(db);

        var recipeId = await sut.CreateRecipeAsync(new DBModel.Recipe
        {
            Title = RecipeTitle.From("Created Recipe"),
            AccessScope = RecipeAccessScope.Private,
        });

        var entity = await db.Recipes
            .AsNoTracking()
            .SingleAsync(r => r.Id == recipeId.Value, cancellationToken);

        entity.CreatedAtUtc.Should().NotBe(default);
        entity.UpdatedAtUtc.Should().NotBe(default);
        entity.UpdatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MenuDbContext(options);
    }
}
