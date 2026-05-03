﻿using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class IngredientService(IUnitRepository unitRepository, IIngredientRepository ingredientRepository) : IIngredientService
{
    public async Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync()
    {
        var ingredientUnits = await unitRepository.GetIngredientUnitsAsync().ConfigureAwait(false);
        return ViewModelMapper.Map(ingredientUnits);
    }

    public async Task<Ingredient> CreateIngredientAsync(NewIngredient newIngredient)
    {
        ArgumentNullException.ThrowIfNull(newIngredient);

        var normalizedIngredient = new NewIngredient
        {
            Name = newIngredient.Name,
            UnitIds = [.. newIngredient.UnitIds.Distinct()],
        };

        return await ingredientRepository.CreateIngredientAsync(normalizedIngredient).ConfigureAwait(false);
    }
}


