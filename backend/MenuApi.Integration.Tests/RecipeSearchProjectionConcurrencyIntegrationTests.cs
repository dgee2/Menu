using System.Text.Json;
using Aspire.Hosting.Testing;
using AwesomeAssertions;
using MenuApi.DomainEvents;
using MenuApi.Integration.Tests.Factory;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuDB;
using MenuDB.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace MenuApi.Integration.Tests;

[Collection("API Host Collection")]
public sealed class RecipeSearchProjectionConcurrencyIntegrationTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task ConcurrentConsumers_LeaveNewerEventPendingUntilOlderBatchCommits()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = await fixture.app.GetConnectionStringAsync("menu", cancellationToken);
        var isolatedConnection = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"SearchConcurrency_{Guid.NewGuid():N}",
        };
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlServer(isolatedConnection.ConnectionString)
            .Options;
        var pause = new PauseBeforeSaveInterceptor();
        var firstOptions = new DbContextOptionsBuilder<MenuDbContext>(options)
            .AddInterceptors(pause)
            .Options;
        await using var db = new MenuDbContext(options);
        await using var firstDb = new MenuDbContext(firstOptions);
        await using var secondDb = new MenuDbContext(options);
        Task<int>? firstPass = null;

        try
        {
            await db.Database.MigrateAsync(cancellationToken);
            var owner = new MenuUserEntity
            {
                Id = Guid.CreateVersion7(),
                AuthSubject = $"concurrent-{Guid.NewGuid():N}",
                DisplayName = "Projection owner",
                CreatedAtUtc = DateTime.UtcNow,
                LastSeenAtUtc = DateTime.UtcNow,
            };
            var recipe = new RecipeEntity
            {
                Id = Guid.CreateVersion7(),
                Title = "Original recipe content",
                OwnerUserId = owner.Id,
                AccessScopeId = (byte)RecipeAccessScope.Private,
            };
            db.MenuUsers.Add(owner);
            db.Recipes.Add(recipe);
            db.RecipeSearchIndexes.Add(new RecipeSearchIndexEntity
            {
                RecipeId = recipe.Id,
                Title = "Older indexed content",
                OwnerUserId = owner.Id,
                AccessScope = "Private",
                UpdatedAtUtc = DateTime.UtcNow,
            });
            db.OutboxEvents.Add(new OutboxEvent
            {
                Id = Guid.CreateVersion7(),
                EventType = nameof(RecipeCreatedEvent),
                Payload = JsonSerializer.Serialize(new RecipeCreatedEvent(RecipeId.From(recipe.Id))),
                CreatedAtUtc = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);

            var firstConsumer = new RecipeSearchProjectionRepository(firstDb);
            var secondConsumer = new RecipeSearchProjectionRepository(secondDb);
            firstPass = firstConsumer.ProcessPendingEventsAsync(cancellationToken);
            await pause.ReachedSave.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);

            // The first consumer has already loaded the old aggregate and is paused before saving.
            recipe.Title = "Latest recipe content";
            db.OutboxEvents.Add(new OutboxEvent
            {
                Id = Guid.CreateVersion7(),
                EventType = nameof(RecipeUpdatedEvent),
                Payload = JsonSerializer.Serialize(new RecipeUpdatedEvent(RecipeId.From(recipe.Id))),
                CreatedAtUtc = DateTime.UtcNow,
            });
            await db.SaveChangesAsync(cancellationToken);

            (await secondConsumer.ProcessPendingEventsAsync(cancellationToken)).Should().Be(0);
            (await db.OutboxEvents.AsNoTracking().CountAsync(x => x.ProcessedAtUtc == null, cancellationToken)).Should().Be(2);

            pause.Release();
            (await firstPass).Should().Be(1);
            (await secondConsumer.ProcessPendingEventsAsync(cancellationToken)).Should().Be(1);
            var projection = await db.RecipeSearchIndexes.AsNoTracking().SingleAsync(cancellationToken);
            projection.Title.Should().Be(recipe.Title);
            (await db.OutboxEvents.AsNoTracking().CountAsync(x => x.ProcessedAtUtc == null, cancellationToken)).Should().Be(0);
        }
        finally
        {
            pause.Release();
            try
            {
                if (firstPass is not null)
                {
                    await firstPass.WaitAsync(TimeSpan.FromSeconds(15), CancellationToken.None);
                }
            }
            finally
            {
                await db.Database.EnsureDeletedAsync(CancellationToken.None);
            }
        }
    }

    private sealed class PauseBeforeSaveInterceptor : SaveChangesInterceptor
    {
        private readonly TaskCompletionSource reachedSave = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource resume = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ReachedSave => reachedSave.Task;

        public void Release() => resume.TrySetResult();

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            reachedSave.TrySetResult();
            await resume.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
