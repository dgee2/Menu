using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MenuApi.ViewModel;

namespace MenuApi.Repositories
{
    public interface IIngredientRepository
    {
        Task<Ingredient> CreateIngredientAsync(NewIngredient newIngredient);

        IAsyncEnumerable<Ingredient> GetIngredientsAsync();

        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string q);

        Task<Ingredient> UpdateIngredientAsync(Ingredient ingredient);
    }
}
