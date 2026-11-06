using System.Text.Json;
using AwesomeAssertions;
using MenuApi.DomainEvents;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuDB;
using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

public class RecipeSearchProjectionRepositoryTests
{
    [Fact]
    public async Task PendingEventForSoftDeletedRecipe_PreservesProjectionForRestoration()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new MenuDbContext(options);
        var recipe = new RecipeEntity
        {
            Id = Guid.CreateVersion7(),
            Title = "Restorable lemon cake",
            OwnerUserId = Guid.CreateVersion7(),
            AccessScopeId = (byte)RecipeAccessScope.Private,
            AccessScope = new RecipeAccessScopeEntity { Id = (byte)RecipeAccessScope.Private, Name = "Private" },
            DeletedAtUtc = DateTime.UtcNow,
        };
        db.Recipes.Add(recipe);
        var outboxEvent = new OutboxEvent
        {
            Id = Guid.CreateVersion7(),
            EventType = nameof(RecipeCreatedEvent),
            Payload = JsonSerializer.Serialize(new RecipeCreatedEvent(RecipeId.From(recipe.Id))),
            CreatedAtUtc = DateTime.UtcNow,
        };
        db.OutboxEvents.Add(outboxEvent);
        await db.SaveChangesAsync(cancellationToken);
        var repository = new RecipeSearchProjectionRepository(db);

        (await repository.ProcessPendingEventsAsync(cancellationToken)).Should().Be(1);

        var projection = await db.RecipeSearchIndexes.SingleOrDefaultAsync(cancellationToken);
        projection.Should().NotBeNull();
        projection?.Title.Should().Be(recipe.Title);
        outboxEvent.ProcessedAtUtc.Should().NotBeNull();
        (await db.Recipes.CountAsync(cancellationToken)).Should().Be(0);

        recipe.DeletedAtUtc = null;
        await db.SaveChangesAsync(cancellationToken);
        (await repository.ProcessPendingEventsAsync(cancellationToken)).Should().Be(0);
        (await db.Recipes.CountAsync(cancellationToken)).Should().Be(1);
        (await db.RecipeSearchIndexes.SingleAsync(cancellationToken)).RecipeId.Should().Be(recipe.Id);
    }
}
