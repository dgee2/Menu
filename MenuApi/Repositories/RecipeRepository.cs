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
    public class RecipeRepository : IRecipeRepository
    {
        private readonly Container recipeContainer;
        private readonly IMapper mapper;
        private readonly ISearchFactory searchFactory;

        public RecipeRepository(CosmosClient cosmosClient, IOptions<CosmosConfig> cosmosConfigOptions, IMapper mapper, ISearchFactory searchFactory)
        {
            if (cosmosClient is null)
            {
                throw new ArgumentNullException(nameof(cosmosClient));
            }

            var cosmosConfig = cosmosConfigOptions?.Value ?? throw new ArgumentNullException(nameof(cosmosConfigOptions));

            recipeContainer = cosmosClient.GetContainer(cosmosConfig.DatabaseId, cosmosConfig.RecipeContainerId);
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.searchFactory = searchFactory ?? throw new ArgumentNullException(nameof(searchFactory));
        }

        public IAsyncEnumerable<ViewModel.Recipe> GetRecipesAsync()
            => recipeContainer
                    .GetItemQueryIterator<DBModel.Recipe>(@"SELECT * FROM c")
                    .ToAsyncEnumerable()
                    .Select(mapper.Map<ViewModel.Recipe>);

        public async Task<ViewModel.Recipe> CreateRecipeAsync(ViewModel.NewRecipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var recipe = mapper.Map<DBModel.Recipe>(newRecipe);
            var response = await recipeContainer.CreateItemAsync(recipe).ConfigureAwait(false);
            return mapper.Map<ViewModel.Recipe>(response.Resource);
        }

        public async Task<ViewModel.Recipe> UpdateRecipeAsync(ViewModel.Recipe newRecipe)
        {
            if (newRecipe is null)
            {
                throw new ArgumentNullException(nameof(newRecipe));
            }

            var recipe = mapper.Map<DBModel.Recipe>(newRecipe);
            var response = await recipeContainer.ReplaceItemAsync(recipe, recipe.Id.ToString()).ConfigureAwait(false);
            return mapper.Map<ViewModel.Recipe>(response.Resource);
        }

        public async Task<IEnumerable<ViewModel.Recipe>> SearchRecipesAsync(string q)
        {
            using var searchClient = searchFactory.CreateRecipeSearchClient();
            var results = await searchClient.Documents.SearchAsync<Recipe>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Recipe>);
        }
    }
}
