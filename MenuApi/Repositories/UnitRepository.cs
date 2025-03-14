﻿using Dapper;
using MenuApi.DBModel;
using Microsoft.Data.SqlClient;

namespace MenuApi.Repositories;

public class UnitRepository(SqlConnection dbConnection) : IUnitRepository
{
    public async Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync()
        => await dbConnection.QueryAsync<IngredientUnit>("dbo.GetIngredientUnits").ConfigureAwait(false);
}
