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
            .Select(i => new IngredientProjection(
                i.Id,
                i.Name,
                i.IngredientUnits.Select(iu => new UnitProjection(iu.Unit.Name, iu.Unit.Abbreviation, iu.Unit.UnitType.Name))))
            .ToListAsync()
            .ConfigureAwait(false);

        return rows.Select(MapToViewModel);
    }

    public async Task<ViewModel.Ingredient> CreateIngredientAsync(ViewModel.NewIngredient newIngredient)
    {
        ArgumentNullException.ThrowIfNull(newIngredient);

        var existing = await db.Ingredients
            .Where(i => i.Name == newIngredient.Name.Value)
            .OrderBy(i => i.Id)
            .Select(i => new IngredientProjection(
                i.Id,
                i.Name,
                i.IngredientUnits.Select(iu => new UnitProjection(iu.Unit.Name, iu.Unit.Abbreviation, iu.Unit.UnitType.Name))))
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return MapToViewModel(existing);
        }

        var unitIds = newIngredient.UnitIds.Distinct().ToList();

        var entity = new IngredientEntity
        {
            Name = newIngredient.Name.Value,
            IngredientUnits = unitIds
                .Select(unitId => new IngredientUnitEntity { UnitId = unitId })
                .ToList(),
        };
        db.Ingredients.Add(entity);
        await db.SaveChangesAsync().ConfigureAwait(false);

        var created = await db.Ingredients
            .Where(i => i.Id == entity.Id)
            .Select(i => new IngredientProjection(
                i.Id,
                i.Name,
                i.IngredientUnits.Select(iu => new UnitProjection(iu.Unit.Name, iu.Unit.Abbreviation, iu.Unit.UnitType.Name))))
            .FirstAsync()
            .ConfigureAwait(false);

        return MapToViewModel(created);
    }

    private static ViewModel.Ingredient MapToViewModel(IngredientProjection p) =>
        new()
        {
            Id = IngredientId.From(p.Id),
            Name = IngredientName.From(p.Name),
            Units = p.Units.Select(u => new ViewModel.IngredientUnit(
                IngredientUnitName.From(u.Name),
                u.Abbreviation is not null ? IngredientUnitAbbreviation.From(u.Abbreviation) : null,
                IngredientUnitType.From(u.UnitType))),
        };

    private sealed record IngredientProjection(int Id, string Name, IEnumerable<UnitProjection> Units);

    private sealed record UnitProjection(string Name, string? Abbreviation, string UnitType);
}
