using MenuApi.Repositories;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IngredientController : ControllerBase
{
    private readonly IIngredientRepository ingredientRepository;

    public IngredientController(IIngredientRepository ingredientRepository)
    {
        this.ingredientRepository = ingredientRepository ?? throw new ArgumentNullException(nameof(ingredientRepository));
    }

    [HttpGet]
    public async Task<IEnumerable<Ingredient>> GetIngredients() => await ingredientRepository.GetIngredientsAsync().ConfigureAwait(false);
}
