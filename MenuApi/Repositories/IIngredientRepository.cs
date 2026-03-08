﻿using MenuApi.ViewModel;

namespace MenuApi.Repositories;

public interface IIngredientRepository
{
    Task<IEnumerable<Ingredient>> GetIngredientsAsync();

    Task<Ingredient> CreateIngredientAsync(NewIngredient newIngredient);
}
