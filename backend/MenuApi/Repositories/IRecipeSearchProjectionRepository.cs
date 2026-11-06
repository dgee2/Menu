namespace MenuApi.Repositories;

public interface IRecipeSearchProjectionRepository
{
    Task<int> ProcessPendingEventsAsync(CancellationToken cancellationToken);
}
