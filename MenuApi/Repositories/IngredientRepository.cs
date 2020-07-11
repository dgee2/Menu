using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MenuApi.Configuration;
using MenuApi.DBModel;
using MenuApi.Extensions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Options;

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

        public async Task<ViewModel.Ingredient> CreateIngredientAsync(ViewModel.NewIngredient newIngredient)
        {
            if (newIngredient is null)
            {
                throw new ArgumentNullException(nameof(newIngredient));
            }

            var ingredient = mapper.Map<DBModel.Ingredient>(newIngredient);
            var response = await ingredientContainer.CreateItemAsync(ingredient).ConfigureAwait(false);
            return mapper.Map<ViewModel.Ingredient>(response.Resource);
        }

        public async Task<ViewModel.Ingredient> UpdateIngredientAsync(ViewModel.Ingredient ingredient)
        {
            if (ingredient is null)
            {
                throw new ArgumentNullException(nameof(ingredient));
            }

            var newIngredient = mapper.Map<DBModel.Ingredient>(ingredient);
            var response = await ingredientContainer.ReplaceItemAsync(newIngredient, newIngredient.Id.ToString()).ConfigureAwait(false);
            return mapper.Map<ViewModel.Ingredient>(response.Resource);
        }

        public async Task<IEnumerable<ViewModel.Ingredient>> SearchIngredientsAsync(string q)
        {
            using var searchClient = searchFactory.CreateIngredientSearchClient();
            var results = await searchClient.Documents.SearchAsync<Ingredient>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Ingredient>);
        }
    }
}
