using MenuApi.DBModel;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MenuApi.Repositories;

public class UnitRepository(MenuDbContext db) : IUnitRepository
{
    public async Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync()
    {
        return await db.Units
            .Select(u => new IngredientUnit
            {
                Name = IngredientUnitName.From(u.Name),
                Abbreviation = u.Abbreviation != null ? IngredientUnitAbbreviation.From(u.Abbreviation) : null,
                UnitType = IngredientUnitType.From(u.UnitType.Name),
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
