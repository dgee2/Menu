using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MenuApi.DBModel;

namespace MenuApi.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly IDbConnection dbConnection;

        public IngredientRepository(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<ViewModel.Ingredient>> GetIngredientsAsync()
            => (await dbConnection.QueryAsync<Ingredient>("dbo.GetIngredients", commandType: CommandType.StoredProcedure).ConfigureAwait(false))
                    .GroupBy(x => (x.Id, x.Name), x => (x.Unit, x.UnitAbbreviation))
                    .Select(x => new ViewModel.Ingredient(x.Key.Id, x.Key.Name, x));
    }
}
