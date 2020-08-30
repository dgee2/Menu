using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using MenuApi.DBModel;
using Microsoft.Azure.Search;

namespace MenuApi.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IMapper mapper;
        private readonly ISearchFactory searchFactory;
        private readonly IDbConnection dbConnection;

        public RecipeRepository(IMapper mapper, ISearchFactory searchFactory, IDbConnection dbConnection)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.searchFactory = searchFactory ?? throw new ArgumentNullException(nameof(searchFactory));
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<ViewModel.Recipe>> GetRecipesAsync()
            => (await dbConnection.QueryAsync<Recipe>("dbo.GetRecipes", commandType: CommandType.StoredProcedure).ConfigureAwait(false))
                .Select(mapper.Map<ViewModel.Recipe>);

        public async Task<ViewModel.Recipe> GetRecipeAsync(int recipeId)
        {
            var recipe = await dbConnection.QueryFirstOrDefaultAsync<Recipe>("dbo.GetRecipe", new { recipeId }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
            return mapper.Map<ViewModel.Recipe>(recipe);
        }

        public async Task<IEnumerable<ViewModel.RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
            => (await dbConnection.QueryAsync<RecipeIngredient>("dbo.GetRecipeIngredients", new { recipeId }, commandType: CommandType.StoredProcedure).ConfigureAwait(false))
                .Select(mapper.Map<ViewModel.RecipeIngredient>);

        public async Task<ViewModel.Recipe> CreateRecipeAsync(string name, IEnumerable<ViewModel.RecipeIngredient> ingredients)
        {
            var recipeId = await dbConnection.ExecuteScalarAsync<int>("dbo.CreateRecipe", new { name }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

            // TODO: Merge ingredients
            return await GetRecipeAsync(recipeId).ConfigureAwait(false);
        }

        public async Task<ViewModel.Recipe> UpdateRecipeAsync(ViewModel.Recipe newRecipe)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ViewModel.Recipe>> SearchRecipesAsync(string q)
        {
            using var searchClient = searchFactory.CreateRecipeSearchClient();
            var results = await searchClient.Documents.SearchAsync<Recipe>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Recipe>);
        }
    }
}
