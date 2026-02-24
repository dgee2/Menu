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
}
