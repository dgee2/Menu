using MenuDB;
using MenuDB.Data;
using MenuApi.Exceptions;
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

        var normalizedUnitIds = newIngredient.UnitIds.Distinct().ToList();
        var existingIngredients = await db.Ingredients
            .Where(i => i.Name == newIngredient.Name.Value)
            .Include(i => i.IngredientUnits)
                .ThenInclude(iu => iu.Unit)
                    .ThenInclude(u => u.UnitType)
            .ToListAsync()
            .ConfigureAwait(false);

        var existingEquivalentIngredient = existingIngredients
            .FirstOrDefault(i => i.IngredientUnits.Select(iu => iu.UnitId).ToHashSet().SetEquals(normalizedUnitIds));

        if (existingEquivalentIngredient is not null)
        {
            return MapIngredient(existingEquivalentIngredient);
        }

        if (existingIngredients.Count != 0)
        {
            throw new ConflictException(
                $"Ingredient '{newIngredient.Name.Value}' already exists with a different unit set.");
        }

        var entity = new IngredientEntity
        {
            Name = newIngredient.Name.Value,
            IngredientUnits = normalizedUnitIds
                .Select(unitId => new IngredientUnitEntity { UnitId = unitId })
                .ToList(),
        };
        db.Ingredients.Add(entity);
        await db.SaveChangesAsync().ConfigureAwait(false);

        var created = await db.Ingredients
            .Where(i => i.Id == entity.Id)
            .Include(i => i.IngredientUnits)
                .ThenInclude(iu => iu.Unit)
                    .ThenInclude(u => u.UnitType)
            .FirstAsync()
            .ConfigureAwait(false);

        return MapIngredient(created);
    }

    private static ViewModel.Ingredient MapIngredient(IngredientEntity ingredient)
    {
        return new ViewModel.Ingredient
        {
            Id = IngredientId.From(ingredient.Id),
            Name = IngredientName.From(ingredient.Name),
            Units = ingredient.IngredientUnits
                .OrderBy(iu => iu.UnitId)
                .Select(iu => new ViewModel.IngredientUnit(
                    IngredientUnitName.From(iu.Unit.Name),
                    iu.Unit.Abbreviation is not null ? IngredientUnitAbbreviation.From(iu.Unit.Abbreviation) : null,
                    IngredientUnitType.From(iu.Unit.UnitType.Name))),
        };
    }
}
