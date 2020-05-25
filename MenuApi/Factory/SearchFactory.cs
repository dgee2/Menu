using System;
using MenuApi.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Options;

namespace MenuApi.Repositories
{
    public class SearchFactory : ISearchFactory
    {
        private readonly SearchConfig searchConfig;

        public SearchFactory(IOptions<SearchConfig> searchConfig)
        {
            this.searchConfig = searchConfig?.Value ?? throw new ArgumentNullException(nameof(searchConfig));
        }

        public ISearchIndexClient CreateIngredientSearchClient()
            => new SearchIndexClient(searchConfig.ServiceName, searchConfig.IngredientIndex, new SearchCredentials(searchConfig.QueryApiKey));

        public ISearchIndexClient CreateRecipeSearchClient()
            => new SearchIndexClient(searchConfig.ServiceName, searchConfig.RecipeIndex, new SearchCredentials(searchConfig.QueryApiKey));
    }
}
