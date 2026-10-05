using System.Text.Json;
using AwesomeAssertions;
using MenuDB;
using MenuApi.DomainEvents;
using MenuApi.Outbox;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Outbox;

public class OutboxWriterTests
{
    [Fact]
    public async Task Write_Queues_Serializable_Events_Without_Marking_Them_Processed()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new MenuDbContext(options);
        var sut = new OutboxWriter(db);
        var recipeId = RecipeId.From(Guid.CreateVersion7());

        sut.Write(new RecipeCreatedEvent(recipeId));
        sut.Write(new RecipeUpdatedEvent(recipeId));

        (await db.OutboxEvents.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var events = await db.OutboxEvents.OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(TestContext.Current.CancellationToken);
        events.Should().HaveCount(2);
        events.Select(x => x.Id).Should().OnlyHaveUniqueItems();
        events.Select(x => x.CreatedAtUtc).Should().AllSatisfy(x => x.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1)));
        events.Select(x => x.ProcessedAtUtc).Should().OnlyContain(x => x == null);

        var created = events.Single(x => x.EventType == nameof(RecipeCreatedEvent));
        var updated = events.Single(x => x.EventType == nameof(RecipeUpdatedEvent));
        using var createdPayload = JsonDocument.Parse(created.Payload);
        createdPayload.RootElement.GetProperty("RecipeId").GetGuid().Should().Be(recipeId.Value);
        using var updatedPayload = JsonDocument.Parse(updated.Payload);
        updatedPayload.RootElement.GetProperty("RecipeId").GetGuid().Should().Be(recipeId.Value);
        JsonSerializer.Deserialize<RecipeCreatedEvent>(created.Payload).Should().Be(new RecipeCreatedEvent(recipeId));
        JsonSerializer.Deserialize<RecipeUpdatedEvent>(updated.Payload).Should().Be(new RecipeUpdatedEvent(recipeId));
    }
}
