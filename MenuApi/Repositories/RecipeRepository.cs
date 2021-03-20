using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapper;
using MenuApi.DBModel;

namespace MenuApi.Repositories
{
    [ExcludeFromCodeCoverage]
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IDbConnection dbConnection;

        public RecipeRepository(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public Task<IEnumerable<Recipe>> GetRecipesAsync() => GetRecipesAsync(null);

        public async Task<IEnumerable<Recipe>> GetRecipesAsync(IDbTransaction? transaction)
            => await dbConnection.QueryAsync<Recipe>("dbo.GetRecipes", commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

        public Task<Recipe> GetRecipeAsync(int recipeId) => GetRecipeAsync(recipeId, null);

        public async Task<Recipe> GetRecipeAsync(int recipeId, IDbTransaction? transaction)
            => await dbConnection.QueryFirstOrDefaultAsync<Recipe>("dbo.GetRecipe", new { recipeId }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

        public Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId) => GetRecipeIngredientsAsync(recipeId, null);

        public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId, IDbTransaction? transaction)
            => await dbConnection.QueryAsync<RecipeIngredient>("dbo.GetRecipeIngredients", new { recipeId }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

        public Task<int> CreateRecipeAsync(string name) => CreateRecipeAsync(name, null);

        public async Task<int> CreateRecipeAsync(string name, IDbTransaction? transaction)
            => await dbConnection.ExecuteScalarAsync<int>("dbo.CreateRecipe", new { name }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

        public Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<RecipeIngredient> recipeIngredients) => UpsertRecipeIngredientsAsync(recipeId, recipeIngredients, null);

        public async Task UpsertRecipeIngredientsAsync(int recipeId, IEnumerable<RecipeIngredient> recipeIngredients, IDbTransaction? transaction)
        {
            if (recipeIngredients == null)
            {
                throw new ArgumentNullException(nameof(recipeIngredients));
            }

            using var dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("unit", typeof(string));
            dt.Columns.Add("amount", typeof(decimal));

            foreach (var ingredient in recipeIngredients)
            {
                dt.Rows.Add(ingredient.Name, ingredient.Unit, ingredient.Amount);
            }

            await dbConnection.ExecuteAsync("dbo.UpsertRecipeIngredients", new { recipeId, tvpIngredients = dt.AsTableValuedParameter() }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);
        }

        public Task UpdateRecipeAsync(int recipeId, string name) => UpdateRecipeAsync(recipeId, name, null);

        public async Task UpdateRecipeAsync(int recipeId, string name, IDbTransaction? transaction)
        {
            await dbConnection.ExecuteAsync("dbo.UpdateRecipe", new { recipeId, name }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);
        }
    }
}
