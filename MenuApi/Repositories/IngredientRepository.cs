using System.Data;
using Dapper;
using MenuApi.DBModel;
using Microsoft.Data.SqlClient;

namespace MenuApi.Repositories;

public class IngredientRepository(SqlConnection dbConnection) : IIngredientRepository
{
    public async Task<IEnumerable<ViewModel.Ingredient>> GetIngredientsAsync()
        => (await dbConnection.QueryAsync<Ingredient>("dbo.GetIngredients", commandType: CommandType.StoredProcedure).ConfigureAwait(false))
                .GroupBy(x => (x.Id, x.Name), x => new ViewModel.IngredientUnit(x.Unit, x.UnitAbbreviation, x.UnitType))
                .Select(x => new ViewModel.Ingredient
                {
                    Id = x.Key.Id,
                    Name = x.Key.Name,
                    Units = x
                });
}
