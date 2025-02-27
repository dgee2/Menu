using MenuApi.ViewModel;

namespace MenuApi.Repositories
{
    public interface IUnitRepository
    {
        Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync();
    }
}