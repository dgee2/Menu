using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.ViewModel;

namespace MenuApi.Repositories
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetIngredientsAsync();
    }
}
