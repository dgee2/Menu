﻿using MenuApi.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenuApi.Repositories
{
    public interface IIngredientRepository
    {
        Task CreateIngredientAsync(NewIngredient newIngredient);
        Task<Ingredient?> GetIngredientAsync(Guid id);
        IAsyncEnumerable<Ingredient> GetIngredientsAsync();
        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string q);
        Task UpdateIngredientAsync(Ingredient ingredient);
    }
}