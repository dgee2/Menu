using MenuApi.DBModel;

namespace MenuApi.Repositories
{
    public interface IUnitRepository
    {
        Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync();
    }
}