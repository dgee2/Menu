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
        private readonly ISearchIndexClient searchClient;

        public IngredientRepository(CosmosClient cosmosClient, IOptions<CosmosConfig> cosmosConfigOptions, IMapper mapper, ISearchIndexClient searchClient)
        {
            if (cosmosClient is null)
            {
                throw new ArgumentNullException(nameof(cosmosClient));
            }

            var cosmosConfig = cosmosConfigOptions?.Value ?? throw new ArgumentNullException(nameof(cosmosConfigOptions));

            ingredientContainer = cosmosClient.GetContainer(cosmosConfig.DatabaseId, cosmosConfig.IngredientContainerId);
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
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

        public Task<ViewModel.Ingredient?> GetIngredientAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ViewModel.Ingredient> UpdateIngredientAsync(ViewModel.Ingredient newIngredient)
        {
            if (newIngredient is null)
            {
                throw new ArgumentNullException(nameof(newIngredient));
            }

            var ingredient = mapper.Map<DBModel.Ingredient>(newIngredient);
            var response = await ingredientContainer.ReplaceItemAsync(ingredient, ingredient.Id.ToString()).ConfigureAwait(false);
            return mapper.Map<ViewModel.Ingredient>(response.Resource);
        }

        public async Task<IEnumerable<ViewModel.Ingredient>> SearchIngredientsAsync(string q)
        {
            var results = await searchClient.Documents.SearchAsync<Ingredient>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Ingredient>);
        }
    }
}
