using AwesomeAssertions;
using FakeItEasy;
using MenuDB;
using MenuDB.Data;
using MenuApi.DomainEvents;
using MenuApi.Outbox;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MenuApi.Tests.Services;

public class RecipeServiceRetryTests
{
    [Fact]
    public async Task CreateRecipe_Retries_After_Rollback_Without_Duplicate_Tracked_Recipe()
    {
        var options = CreateOptions();
        await using var db = new MenuDbContext(options);
        var repository = A.Fake<IRecipeRepository>();
        var steps = A.Fake<IRecipeStepRepository>();
        var callerId = MenuUserId.From(Guid.CreateVersion7());
        var attempts = 0;

        A.CallTo(() => repository.CreateRecipeAsync(A<DBModel.Recipe>._, A<RecipeId?>._))
            .Invokes((DBModel.Recipe recipe, RecipeId? id) =>
            {
                db.Recipes.Add(NewEntity(id!.Value, recipe.Title.Value, callerId));
                db.SaveChanges();
            })
            .ReturnsLazily((DBModel.Recipe _, RecipeId? id) => id!.Value);
        A.CallTo(() => steps.UpsertStepCollectionAsync(A<RecipeId>._, A<IEnumerable<DBModel.RecipeStep>>._))
            .Invokes(() =>
            {
                if (attempts++ == 0)
                {
                    // InMemory ignores transactions, so remove the first attempt's rows to
                    // simulate a database rollback while leaving its EF entries tracked.
                    db.Database.EnsureDeleted();
                    throw new SimulatedTransientException();
                }
            });

        var sut = NewService(db, repository, steps);
        var recipeId = await sut.CreateRecipeAsync(NewRecipe("Created after retry"), callerId);

        attempts.Should().Be(2);
        (await db.Recipes.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).Id.Should().Be(recipeId.Value);
        var outboxEvent = await db.OutboxEvents.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        outboxEvent.EventType.Should().Be(nameof(RecipeCreatedEvent));
        System.Text.Json.JsonSerializer.Deserialize<RecipeCreatedEvent>(outboxEvent.Payload)!.RecipeId.Should().Be(recipeId);
    }

    [Fact]
    public async Task UpdateRecipe_Retries_After_Rollback_And_Reapplies_Recipe_Fields()
    {
        var options = CreateOptions();
        await using var db = new MenuDbContext(options);
        var repository = A.Fake<IRecipeRepository>();
        var steps = A.Fake<IRecipeStepRepository>();
        var callerId = MenuUserId.From(Guid.CreateVersion7());
        var recipeId = RecipeId.From(Guid.CreateVersion7());
        const string originalTitle = "Before retry";
        db.Recipes.Add(NewEntity(recipeId, originalTitle, callerId));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        A.CallTo(() => repository.GetRecipeAsync(recipeId)).Returns(new DBModel.Recipe
        {
            Id = recipeId,
            Title = RecipeTitle.From(originalTitle),
            AccessScope = RecipeAccessScope.Private,
            OwnerUserId = callerId,
        });
        A.CallTo(() => repository.UpdateRecipeAsync(recipeId, A<DBModel.Recipe>._))
            .Invokes((RecipeId _, DBModel.Recipe recipe) =>
            {
                var entity = db.Recipes.Single(r => r.Id == recipeId.Value);
                entity.Title = recipe.Title.Value;
                db.SaveChanges();
            });

        var attempts = 0;
        A.CallTo(() => steps.UpsertStepCollectionAsync(recipeId, A<IEnumerable<DBModel.RecipeStep>>._))
            .Invokes(() =>
            {
                if (attempts++ == 0)
                {
                    db.Database.EnsureDeleted();
                    using var resetDb = new MenuDbContext(options);
                    resetDb.Recipes.Add(NewEntity(recipeId, originalTitle, callerId));
                    resetDb.SaveChanges();
                    throw new SimulatedTransientException();
                }
            });

        var sut = NewService(db, repository, steps);
        (await sut.UpdateRecipeAsync(recipeId, NewRecipe("After retry"), callerId)).Should().BeTrue();

        attempts.Should().Be(2);
        (await db.Recipes.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).Title.Should().Be("After retry");
        var outboxEvent = await db.OutboxEvents.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        outboxEvent.EventType.Should().Be(nameof(RecipeUpdatedEvent));
        System.Text.Json.JsonSerializer.Deserialize<RecipeUpdatedEvent>(outboxEvent.Payload)!.RecipeId.Should().Be(recipeId);
    }

    private static DbContextOptions<MenuDbContext> CreateOptions() => new DbContextOptionsBuilder<MenuDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ReplaceService<IExecutionStrategyFactory, RetryOnceStrategyFactory>()
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    private static RecipeService NewService(MenuDbContext db, IRecipeRepository repository, IRecipeStepRepository steps) =>
        new(repository, steps, db, new OutboxWriter(db), NullLogger<RecipeService>.Instance);

    private static UpsertRecipe NewRecipe(string title) => new()
    {
        Title = RecipeTitle.From(title),
        AccessScope = RecipeAccessScope.Private,
        Ingredients = [],
        Steps = [],
    };

    private static RecipeEntity NewEntity(RecipeId recipeId, string title, MenuUserId ownerId) => new()
    {
        Id = recipeId.Value,
        Title = title,
        OwnerUserId = ownerId.Value,
        AccessScopeId = (byte)RecipeAccessScope.Private,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
    };

    public sealed class RetryOnceStrategyFactory(ExecutionStrategyDependencies dependencies) : IExecutionStrategyFactory
    {
        public IExecutionStrategy Create() => new RetryOnceStrategy(dependencies);
    }

    private sealed class RetryOnceStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, 1, TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => exception is SimulatedTransientException;
    }

    private sealed class SimulatedTransientException : Exception;
}
