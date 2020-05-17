using Microsoft.Azure.Search;

namespace MenuApi.Search
{
    public interface ISearchFactory
    {
        ISearchServiceClient CreateSearchServiceClient();
        ISearchIndexClient CreateSearchIndexClient();
    }
}