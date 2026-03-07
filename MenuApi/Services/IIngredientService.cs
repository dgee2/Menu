﻿using MenuApi.ViewModel;

namespace MenuApi.Services;

public interface IIngredientService
{
    Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync();

    Task<Ingredient> CreateIngredientAsync(NewIngredient newIngredient);
}