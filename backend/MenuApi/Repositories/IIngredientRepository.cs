﻿using MenuApi.ValueObjects;
using MenuApi.ViewModel;

namespace MenuApi.Repositories;

public interface IIngredientRepository
{
    Task<IEnumerable<Ingredient>> GetIngredientsAsync();

    Task<ExistingIngredientLookup?> FindByNameAsync(IngredientName name);

    Task<Ingredient> CreateIngredientAsync(NewIngredient newIngredient);
}
