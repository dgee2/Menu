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
    public class IngredientRepository : IIngredientRepository
    {
        private readonly IMapper mapper;
        private readonly ISearchFactory searchFactory;
        private readonly IDbConnection dbConnection;

        public IngredientRepository(IMapper mapper, ISearchFactory searchFactory, IDbConnection dbConnection)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.searchFactory = searchFactory ?? throw new ArgumentNullException(nameof(searchFactory));
            this.dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        }

        public async Task<IEnumerable<ViewModel.Ingredient>> GetIngredientsAsync()
            => (await dbConnection.QueryAsync<Ingredient>("dbo.GetIngredients", commandType: CommandType.StoredProcedure).ConfigureAwait(false))
                    .GroupBy(x => (x.Id, x.Name), x => (x.Unit, x.Abbreviation))
                    .Select(x => new ViewModel.Ingredient(x.Key.Id, x.Key.Name, x));

        public async Task<IEnumerable<ViewModel.Ingredient>> SearchIngredientsAsync(string q)
        {
            using var searchClient = searchFactory.CreateIngredientSearchClient();
            var results = await searchClient.Documents.SearchAsync<Ingredient>(q).ConfigureAwait(false);
            return results.Results.Select(x => x.Document).Select(mapper.Map<ViewModel.Ingredient>);
        }
    }
}
