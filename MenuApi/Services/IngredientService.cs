using MenuApi.MappingProfiles;
using MenuApi.Repositories;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class IngredientService(IUnitRepository unitRepository) : IIngredientService
{
    public async Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync()
    {
        var ingredientUnits = await unitRepository.GetIngredientUnitsAsync().ConfigureAwait(false);
        return ViewModelMapper.Map(ingredientUnits);
    }
}


