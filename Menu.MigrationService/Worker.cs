using System.Diagnostics;
using MenuDB;
using Microsoft.EntityFrameworkCore;

namespace Menu.MigrationService;

internal sealed class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MenuDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private async Task RunMigrationAsync(MenuDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            logger.LogInformation("Applying pending migrations");
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Migrations applied successfully");
        });
    }
}
