using MenuApi.Repositories;

namespace MenuApi.Outbox;

/// <summary>
/// Maintains the denormalized full-text search projection from committed recipe events.
/// </summary>
public sealed class RecipeSearchProjectionConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<RecipeSearchProjectionConsumer> logger) : BackgroundService
{
    private readonly TimeSpan pollingInterval = TimeSpan.FromSeconds(
        Math.Clamp(configuration.GetValue("RecipeSearchProjection:PollingIntervalSeconds", 5), 1, 3600));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await ProcessPendingEventsAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to update recipe search projection from outbox events.");
            }
        }
    }

    private async Task ProcessPendingEventsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRecipeSearchProjectionRepository>();
        var processedCount = await repository.ProcessPendingEventsAsync(cancellationToken).ConfigureAwait(false);
        if (processedCount > 0)
        {
            logger.LogInformation("Processed {EventCount} recipe search projection event(s).", processedCount);
        }
    }
}
