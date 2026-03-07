﻿using MenuDB;
using MenuDB.Data;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public class IngredientRepository(MenuDbContext db) : IIngredientRepository
{
    public async Task<IEnumerable<ViewModel.Ingredient>> GetIngredientsAsync()
    {
        var rows = await db.Ingredients
            .Select(i => new
            {
                i.Id,
                i.Name,
                Units = i.IngredientUnits.Select(iu => new
                {
                    iu.Unit.Name,
                    iu.Unit.Abbreviation,
                    UnitType = iu.Unit.UnitType.Name,
                })
            })
            .ToListAsync()
            .ConfigureAwait(false);

        return rows.Select(i => new ViewModel.Ingredient
        {
            Id = IngredientId.From(i.Id),
            Name = IngredientName.From(i.Name),
            Units = i.Units.Select(u => new ViewModel.IngredientUnit(
                IngredientUnitName.From(u.Name),
                u.Abbreviation is not null ? IngredientUnitAbbreviation.From(u.Abbreviation) : null,
                IngredientUnitType.From(u.UnitType))),
        });
    }

    public async Task<ViewModel.Ingredient> CreateIngredientAsync(ViewModel.NewIngredient newIngredient)
    {
        ArgumentNullException.ThrowIfNull(newIngredient);

        var entity = new IngredientEntity { Name = newIngredient.Name.Value };
        db.Ingredients.Add(entity);
        await db.SaveChangesAsync().ConfigureAwait(false);

        foreach (var unitId in newIngredient.UnitIds)
        {
            db.IngredientUnits.Add(new IngredientUnitEntity
            {
                IngredientId = entity.Id,
                UnitId = unitId,
            });
        }

        await db.SaveChangesAsync().ConfigureAwait(false);

        var created = await db.Ingredients
            .Where(i => i.Id == entity.Id)
            .Select(i => new
            {
                i.Id,
                i.Name,
                Units = i.IngredientUnits.Select(iu => new
                {
                    iu.Unit.Name,
                    iu.Unit.Abbreviation,
                    UnitType = iu.Unit.UnitType.Name,
                })
            })
            .FirstAsync()
            .ConfigureAwait(false);

        return new ViewModel.Ingredient
        {
            Id = IngredientId.From(created.Id),
            Name = IngredientName.From(created.Name),
            Units = created.Units.Select(u => new ViewModel.IngredientUnit(
                IngredientUnitName.From(u.Name),
                u.Abbreviation is not null ? IngredientUnitAbbreviation.From(u.Abbreviation) : null,
                IngredientUnitType.From(u.UnitType))),
        };
    }
}
