using Microsoft.Azure.Search;

namespace MenuApi.Repositories
{
    public interface ISearchFactory
    {
        ISearchIndexClient CreateIngredientSearchClient();

        ISearchIndexClient CreateRecipeSearchClient();
    }
}
