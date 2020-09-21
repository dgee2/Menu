using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MenuApi.DBModel;
using Microsoft.Azure.Search;

namespace MenuApi.Repositories
{
    [ExcludeFromCodeCoverage]
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ISearchFactory searchFactory;
        private readonly IDbConnection dbConnection;

        public RecipeRepository(ISearchFactory searchFactory, IDbConnection dbConnection)
        {
            this.searchFactory = searchFactory ?? throw new ArgumentNullException(nameof(searchFactory));
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<Recipe>> GetRecipesAsync()
            => await dbConnection.QueryAsync<Recipe>("dbo.GetRecipes", commandType: CommandType.StoredProcedure).ConfigureAwait(false);

        public async Task<Recipe> GetRecipeAsync(int recipeId)
            => await dbConnection.QueryFirstOrDefaultAsync<Recipe>("dbo.GetRecipe", new { recipeId }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
            => await dbConnection.QueryAsync<RecipeIngredient>("dbo.GetRecipeIngredients", new { recipeId }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

        public async Task<int> CreateRecipeAsync(string name)
            => await dbConnection.ExecuteScalarAsync<int>("dbo.CreateRecipe", new { name }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

        public async Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<RecipeIngredient> recipeIngredients)
            => await dbConnection.ExecuteAsync("dbo.UpsertRecipeIngredients", new { recipeId, recipeIngredients }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);

        public async Task UpdateRecipeAsync(int recipeId, string name)
        {
            await dbConnection.ExecuteAsync("dbo.UpdateRecipe", new { recipeId, name }, commandType: CommandType.StoredProcedure).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Recipe>> SearchRecipesAsync(string q)
        {
            using var searchClient = searchFactory.CreateRecipeSearchClient();
            var results = await searchClient.Documents.SearchAsync<Recipe>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document);
        }
    }
}
