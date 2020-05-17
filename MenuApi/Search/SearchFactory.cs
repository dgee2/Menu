using MenuApi.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Options;

namespace MenuApi.Search
{
    public class SearchFactory : ISearchFactory
    {
        private readonly SearchConfig config;

        public SearchFactory(IOptions<SearchConfig> config)
        {
            if (config is null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }
            this.config = config.Value;
        }

        public ISearchIndexClient CreateSearchIndexClient()
        {
            SearchIndexClient indexClient = new SearchIndexClient(config.ServiceName, config.IngredientIndex, new SearchCredentials(config.QueryApiKey));
            return indexClient;
        }

        public ISearchServiceClient CreateSearchServiceClient()
            => new SearchServiceClient(config.ServiceName, new SearchCredentials(config.AdminApiKey));
    }
}
