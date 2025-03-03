using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using MenuApi.DBModel;
using MenuApi.ValueObjects;
using Microsoft.Data.SqlClient;

namespace MenuApi.Repositories;

[ExcludeFromCodeCoverage]
public class RecipeRepository(SqlConnection dbConnection) : IRecipeRepository
{
    public Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync() => GetRecipesAsync(null);

    public async Task<IEnumerable<DBModel.Recipe>> GetRecipesAsync(IDbTransaction? transaction)
        => await dbConnection.QueryAsync<DBModel.Recipe>("dbo.GetRecipes", commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

    public Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId) => GetRecipeAsync(recipeId, null);

    public async Task<DBModel.Recipe?> GetRecipeAsync(RecipeId recipeId, IDbTransaction? transaction)
        => await dbConnection.QueryFirstOrDefaultAsync<DBModel.Recipe>("dbo.GetRecipe", new { recipeId }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

    public Task<IEnumerable<GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId) => GetRecipeIngredientsAsync(recipeId, null);

    public async Task<IEnumerable<GetRecipeIngredient>> GetRecipeIngredientsAsync(RecipeId recipeId, IDbTransaction? transaction)
        => await dbConnection.QueryAsync<GetRecipeIngredient>("dbo.GetRecipeIngredients", new { recipeId }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

    public Task<RecipeId> CreateRecipeAsync(RecipeName name) => CreateRecipeAsync(name, null);

    public async Task<RecipeId> CreateRecipeAsync(RecipeName name, IDbTransaction? transaction)
        => await dbConnection.ExecuteScalarAsync<RecipeId>("dbo.CreateRecipe", new { name }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);

    public Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients) => UpsertRecipeIngredientsAsync(recipeId, recipeIngredients, null);

    public async Task UpsertRecipeIngredientsAsync(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> recipeIngredients, IDbTransaction? transaction)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredients);

        using var dt = new DataTable();
        dt.Columns.Add("name", typeof(string));
        dt.Columns.Add("unit", typeof(string));
        dt.Columns.Add("amount", typeof(decimal));

        foreach (var ingredient in recipeIngredients)
        {
            dt.Rows.Add(ingredient.IngredientName.Value, ingredient.UnitName.Value, ingredient.Amount.Value);
        }

        await dbConnection.ExecuteAsync("dbo.UpsertRecipeIngredients", new { recipeId, tvpIngredients = dt.AsTableValuedParameter() }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);
    }

    public Task UpdateRecipeAsync(RecipeId recipeId, RecipeName name) => UpdateRecipeAsync(recipeId, name, null);

    public async Task UpdateRecipeAsync(RecipeId recipeId, RecipeName name, IDbTransaction? transaction)
    {
        await dbConnection.ExecuteAsync("dbo.UpdateRecipe", new { recipeId, name }, commandType: CommandType.StoredProcedure, transaction: transaction).ConfigureAwait(false);
    }
}
