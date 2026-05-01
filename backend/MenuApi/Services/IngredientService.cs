﻿using MenuApi.Exceptions;
using MenuApi.MappingProfiles;
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

        var effectiveUnitIds = newIngredient.UnitIds.Distinct().ToHashSet();

        var existing = await ingredientRepository.FindByNameAsync(newIngredient.Name).ConfigureAwait(false);
        if (existing is not null)
        {
            if (existing.UnitIds.SetEquals(effectiveUnitIds))
            {
                return existing.Ingredient;
            }

            throw new ConflictException(
                $"An ingredient named '{newIngredient.Name.Value}' already exists with a different set of units.");
        }

        return await ingredientRepository.CreateIngredientAsync(newIngredient).ConfigureAwait(false);
    }
}

