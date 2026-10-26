using Aspire.Hosting.Testing;
using AwesomeAssertions;
using MenuApi.Integration.Tests.Factory;
using MenuDB;
using MenuDB.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public sealed class RecipeSearchBackfillIntegrationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task Upgrade_BackfillsRecipesWithoutEventsAndPreservesExistingProjections()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = await fixture.app.GetConnectionStringAsync("menu", cancellationToken);
        var isolatedConnection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"SearchBackfill_{Guid.NewGuid():N}",
        };
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlServer(isolatedConnection.ConnectionString)
            .Options;
        await using var db = new MenuDbContext(options);

        try
        {
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync("20261024201314_AddRecipeSearchIndex", cancellationToken);
            var owner = new MenuUserEntity
            {
                Id = Guid.CreateVersion7(),
                AuthSubject = $"backfill-{Guid.NewGuid():N}",
                DisplayName = "Backfill owner",
                CreatedAtUtc = DateTime.UtcNow,
                LastSeenAtUtc = DateTime.UtcNow,
            };
            db.MenuUsers.Add(owner);
            var active = CreateRecipe(owner.Id, "Legacy lemon cake");
            active.Summary = "A legacy summary";
            active.RecipeIngredients =
            [
                new() { Id = Guid.CreateVersion7(), SortOrder = 1, IngredientText = "lemon zest", MeasureText = "1 tbsp" },
                new() { Id = Guid.CreateVersion7(), SortOrder = 0, IngredientText = "plain flour", MeasureText = "200 g" },
            ];
            active.RecipeSteps =
            [
                new() { Id = Guid.CreateVersion7(), SortOrder = 1, InstructionText = "Bake the cake." },
                new() { Id = Guid.CreateVersion7(), SortOrder = 0, InstructionText = "Mix the batter." },
            ];
            var deleted = CreateRecipe(owner.Id, "Deleted legacy cake");
            deleted.DeletedAtUtc = DateTime.UtcNow;
            var projected = CreateRecipe(owner.Id, "Already projected cake");
            db.Recipes.AddRange(active, deleted, projected);
            db.RecipeSearchIndexes.Add(new RecipeSearchIndexEntity
            {
                RecipeId = projected.Id,
                Title = "Preserve existing projection",
                OwnerUserId = owner.Id,
                AccessScope = "Private",
                IsPublishedExternally = true,
                UpdatedAtUtc = projected.UpdatedAtUtc,
            });
            await db.SaveChangesAsync(cancellationToken);
            (await db.OutboxEvents.CountAsync(cancellationToken)).Should().Be(0);

            await migrator.MigrateAsync(cancellationToken: cancellationToken);
            db.ChangeTracker.Clear();

            var index = await db.RecipeSearchIndexes.SingleAsync(x => x.RecipeId == active.Id, cancellationToken);
            index.Title.Should().Be(active.Title);
            index.SummaryText.Should().Be(active.Summary);
            index.IngredientsText.Should().Be("plain flour lemon zest");
            index.StepsText.Should().Be("Mix the batter. Bake the cake.");
            index.OwnerUserId.Should().Be(owner.Id);
            index.AccessScope.Should().Be("Private");
            index.UpdatedAtUtc.Should().Be(active.UpdatedAtUtc);
            index.IsPublishedExternally.Should().BeFalse();
            (await db.RecipeSearchIndexes.AnyAsync(x => x.RecipeId == deleted.Id, cancellationToken)).Should().BeTrue();
            var existing = await db.RecipeSearchIndexes.SingleAsync(x => x.RecipeId == projected.Id, cancellationToken);
            existing.Title.Should().Be("Preserve existing projection");
            existing.IsPublishedExternally.Should().BeTrue();
        }
        finally
        {
            await db.Database.EnsureDeletedAsync(CancellationToken.None);
        }
    }

    private static RecipeEntity CreateRecipe(Guid ownerId, string title) => new()
    {
        Id = Guid.CreateVersion7(),
        Title = title,
        OwnerUserId = ownerId,
        AccessScopeId = 1,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
    };
}
