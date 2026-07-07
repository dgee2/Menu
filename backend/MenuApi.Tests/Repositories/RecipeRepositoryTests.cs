using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Xunit;

namespace MenuApi.Tests.Repositories;

public class RecipeRepositoryTests
{
    [Fact]
    public async Task CreateRecipeAsync_Sets_Audit_Timestamps()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var testDatabase = await CreateDbContextAsync(cancellationToken);
        await using var db = testDatabase.DbContext;
        var sut = new RecipeRepository(db);

        var recipeId = await sut.CreateRecipeAsync(RecipeTitle.From("Created Recipe"));

        var entity = await db.Recipes
            .AsNoTracking()
            .SingleAsync(r => r.Id == recipeId.Value, cancellationToken);

        entity.CreatedAtUtc.Should().NotBeNull();
        entity.UpdatedAtUtc.Should().NotBeNull();
        entity.UpdatedAtUtc.Should().Be(entity.CreatedAtUtc);
    }

    [Fact]
    public async Task UpdateRecipeAsync_Refreshes_UpdatedAtUtc()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var testDatabase = await CreateDbContextAsync(cancellationToken);
        await using var db = testDatabase.DbContext;

        var originalTimestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var entity = new RecipeEntity
        {
            Title = "Original Recipe",
            AccessScope = "Private",
            CreatedAtUtc = originalTimestamp,
            UpdatedAtUtc = originalTimestamp,
        };

        db.Recipes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var sut = new RecipeRepository(db);
        await sut.UpdateRecipeAsync(RecipeId.From(entity.Id), RecipeTitle.From("Updated Recipe"));

        var updatedEntity = await db.Recipes
            .AsNoTracking()
            .SingleAsync(r => r.Id == entity.Id, cancellationToken);

        updatedEntity.Title.Should().Be("Updated Recipe");
        updatedEntity.CreatedAtUtc.Should().Be(originalTimestamp);
        updatedEntity.UpdatedAtUtc.Should().NotBeNull();
        updatedEntity.UpdatedAtUtc.Should().BeAfter(originalTimestamp);
    }

    private static async Task<TestDatabase> CreateDbContextAsync(CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new MenuDbContext(options);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE Recipe (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                OwnerUserId INTEGER NULL,
                AccessScope TEXT NOT NULL,
                Summary TEXT NULL,
                Servings INTEGER NULL,
                YieldText TEXT NULL,
                PrepTimeMinutes INTEGER NULL,
                CookTimeMinutes INTEGER NULL,
                TotalTimeMinutes INTEGER NULL,
                CreatedAtUtc TEXT NULL,
                UpdatedAtUtc TEXT NULL
            );
            """,
            cancellationToken);

        return new TestDatabase(connection, dbContext);
    }

    private sealed class TestDatabase(SqliteConnection connection, MenuDbContext dbContext) : IAsyncDisposable
    {
        public MenuDbContext DbContext { get; } = dbContext;

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
