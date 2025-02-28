using MenuApi.ViewModel;

namespace MenuApi.Services
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync();
    }
}