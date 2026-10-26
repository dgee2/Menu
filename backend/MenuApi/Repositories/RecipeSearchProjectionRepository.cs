using System.Data;
using System.Text.Json;
using MenuApi.DomainEvents;
using MenuDB;
using MenuDB.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public sealed class RecipeSearchProjectionRepository(MenuDbContext db) : IRecipeSearchProjectionRepository
{
    private const int BatchSize = 100;
    private static readonly string[] RecipeEventTypes = [nameof(RecipeCreatedEvent), nameof(RecipeUpdatedEvent)];

    public async Task<int> ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        // Nonrelational providers do not have competing database connections or SQL locks.
        if (!db.Database.IsRelational())
        {
            return await ProcessPendingBatchAsync(cancellationToken).ConfigureAwait(false);
        }

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // Re-read the batch after a rolled-back attempt; do not reuse tracked acknowledgements.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            var lockResultParameter = new SqlParameter("@lockResult", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output,
            };
            await db.Database.ExecuteSqlRawAsync(
                """
                EXEC @lockResult = sys.sp_getapplock
                    @Resource = N'Menu.RecipeSearchProjection',
                    @LockMode = N'Exclusive',
                    @LockOwner = N'Transaction',
                    @LockTimeout = 0;
                """,
                [lockResultParameter],
                cancellationToken).ConfigureAwait(false);

            var lockResult = (int)lockResultParameter.Value;
            if (lockResult == -1)
            {
                // Another API instance owns the batch. Leave its events untouched until a later tick.
                return 0;
            }

            if (lockResult < 0)
            {
                throw new InvalidOperationException($"Unable to acquire the recipe projection lock: {lockResult}.");
            }

            // The database-wide transaction lock covers reads, rebuilds, and acknowledgements.
            // A second consumer cannot overwrite a newer projection using an older loaded aggregate.
            var processedCount = await ProcessPendingBatchAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return processedCount;
        }).ConfigureAwait(false);
    }

    private async Task<int> ProcessPendingBatchAsync(CancellationToken cancellationToken)
    {
        var events = await db.OutboxEvents
            .Where(e => e.ProcessedAtUtc == null && RecipeEventTypes.Contains(e.EventType))
            .OrderBy(e => e.CreatedAtUtc)
            .ThenBy(e => e.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (events.Count == 0)
        {
            return 0;
        }

        var projectedRecipeIds = new HashSet<Guid>();
        foreach (var outboxEvent in events)
        {
            var recipeId = GetRecipeId(outboxEvent);
            if (!projectedRecipeIds.Add(recipeId))
            {
                // A create and one or more updates may all be pending in the same batch. The
                // projection is rebuilt from current aggregate state, so one rebuild is enough.
                outboxEvent.ProcessedAtUtc = DateTime.UtcNow;
                continue;
            }

            var recipe = await db.Recipes
                // Keep projections for soft-deleted recipes so restoration needs no new event.
                // Search checks DeletedAtUtc on the authoritative recipe before returning rows.
                .IgnoreQueryFilters()
                .Include(r => r.AccessScope)
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .AsSplitQuery()
                .SingleOrDefaultAsync(r => r.Id == recipeId, cancellationToken)
                .ConfigureAwait(false);

            var index = await db.RecipeSearchIndexes
                .SingleOrDefaultAsync(i => i.RecipeId == recipeId, cancellationToken)
                .ConfigureAwait(false);

            if (recipe is null)
            {
                // The recipe may have been physically removed after the event was written. Its
                // search row has a cascading FK in that case, but remove any stale row defensively.
                if (index is not null)
                {
                    db.RecipeSearchIndexes.Remove(index);
                }
            }
            else
            {
                var projection = new RecipeSearchIndexEntity
                {
                    RecipeId = recipe.Id,
                    Title = recipe.Title,
                    SummaryText = recipe.Summary,
                    IngredientsText = JoinInOrder(recipe.RecipeIngredients.OrderBy(x => x.SortOrder).Select(x => x.IngredientText)),
                    StepsText = JoinInOrder(recipe.RecipeSteps.OrderBy(x => x.SortOrder).Select(x => x.InstructionText)),
                    OwnerUserId = recipe.OwnerUserId,
                    AccessScope = recipe.AccessScope?.Name ?? throw new InvalidOperationException(
                        $"Recipe {recipe.Id} has no configured access scope."),
                    IsPublishedExternally = index?.IsPublishedExternally ?? false,
                    UpdatedAtUtc = recipe.UpdatedAtUtc,
                };

                if (index is null)
                {
                    db.RecipeSearchIndexes.Add(projection);
                }
                else
                {
                    index.Title = projection.Title;
                    index.SummaryText = projection.SummaryText;
                    index.IngredientsText = projection.IngredientsText;
                    index.StepsText = projection.StepsText;
                    index.OwnerUserId = projection.OwnerUserId;
                    index.AccessScope = projection.AccessScope;
                    index.UpdatedAtUtc = projection.UpdatedAtUtc;
                }
            }

            outboxEvent.ProcessedAtUtc = DateTime.UtcNow;
        }

        // Projection changes and acknowledgement are persisted together, so an interrupted batch
        // can safely be retried without losing an event or acknowledging an incomplete projection.
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return events.Count;
    }

    private static Guid GetRecipeId(OutboxEvent outboxEvent)
    {
        var recipeId = outboxEvent.EventType switch
        {
            nameof(RecipeCreatedEvent) => JsonSerializer.Deserialize<RecipeCreatedEvent>(outboxEvent.Payload)?.RecipeId.Value,
            nameof(RecipeUpdatedEvent) => JsonSerializer.Deserialize<RecipeUpdatedEvent>(outboxEvent.Payload)?.RecipeId.Value,
            _ => null,
        };

        return recipeId is { } value && value != Guid.Empty
            ? value
            : throw new JsonException($"Outbox event {outboxEvent.Id} has an invalid recipe id payload.");
    }

    private static string? JoinInOrder(IEnumerable<string> values)
    {
        var text = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
        return text.Length == 0 ? null : text;
    }
}
