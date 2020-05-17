using AutoMapper;
using MenuApi.Configuration;
using MenuApi.DBModel;
using MenuApi.Extensions;
using MenuApi.Search;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MenuApi.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly Container ingredientContainer;
        private readonly IMapper mapper;
        private readonly ISearchFactory searchFactory;

        public IngredientRepository(CosmosClient cosmosClient, IOptions<CosmosConfig> cosmosConfigOptions, IMapper mapper, ISearchFactory searchFactory)
        {
            if (cosmosClient is null)
            {
                throw new ArgumentNullException(nameof(cosmosClient));
            }

            var cosmosConfig = cosmosConfigOptions?.Value ?? throw new ArgumentNullException(nameof(cosmosConfigOptions));

            ingredientContainer = cosmosClient.GetContainer(cosmosConfig.DatabaseId, cosmosConfig.IngredientContainerId);
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.searchFactory = searchFactory ?? throw new ArgumentNullException(nameof(searchFactory));
        }

        public IAsyncEnumerable<ViewModel.Ingredient> GetIngredientsAsync()
            => ingredientContainer
                    .GetItemQueryIterator<DBModel.Ingredient>(@"SELECT * FROM c")
                    .ToAsyncEnumerable()
                    .Select(mapper.Map<ViewModel.Ingredient>);

        public async Task CreateIngredientAsync(ViewModel.NewIngredient newIngredient)
        {
            if (newIngredient is null)
            {
                throw new ArgumentNullException(nameof(newIngredient));
            }

            var ingredient = mapper.Map<DBModel.Ingredient>(newIngredient);
            await ingredientContainer.CreateItemAsync(ingredient).ConfigureAwait(false);
        }

        public Task<ViewModel.Ingredient?> GetIngredientAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateIngredientAsync(ViewModel.Ingredient ingredient)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ViewModel.Ingredient>> SearchIngredientsAsync(string q)
        {
            var search = searchFactory.CreateSearchIndexClient();
            var results = await search.Documents.SearchAsync<Ingredient>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Ingredient>);
        }
    }
}
