using AutoMapper;
using MenuApi.Repositories;
using MenuApi.ViewModel;

namespace MenuApi.Services;

public class IngredientService(IUnitRepository unitRepository, IMapper mapper) : IIngredientService
{
    public async Task<IEnumerable<IngredientUnit>> GetIngredientUnitsAsync()
    {
        var ingredientUnits = await unitRepository.GetIngredientUnitsAsync().ConfigureAwait(false);
        return mapper.Map<IEnumerable<IngredientUnit>>(ingredientUnits);
    }
}


