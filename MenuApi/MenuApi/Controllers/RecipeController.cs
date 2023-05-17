using MenuApi.Services;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MenuApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService recipeService;

    public RecipeController(IRecipeService recipeService)
    {
        this.recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
    }

    [HttpGet]
    public async Task<IEnumerable<Recipe>> GetRecipesAsync()
        => await recipeService.GetRecipesAsync().ConfigureAwait(false);

    [HttpGet("{recipeId}")]
    public async Task<FullRecipe> GetRecipeAsync(int recipeId)
        => await recipeService.GetRecipeAsync(recipeId).ConfigureAwait(false);

    [HttpGet("{recipeId}/Ingredient")]
    public async Task<IEnumerable<RecipeIngredient>> GetRecipeIngredientsAsync(int recipeId)
        => await recipeService.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

    [HttpPost]
    public async Task<FullRecipe> CreateRecipeAsync([FromBody] NewRecipe newRecipe)
    {
        var recipeId = await recipeService.CreateRecipeAsync(newRecipe).ConfigureAwait(false);
        return await recipeService.GetRecipeAsync(recipeId).ConfigureAwait(false);
    }

    [HttpPut("{recipeId}")]
    public async Task<FullRecipe> UpdateRecipeAsync(int recipeId, [FromBody] NewRecipe newRecipe)
    {
        await recipeService.UpdateRecipeAsync(recipeId, newRecipe).ConfigureAwait(false);
        return await recipeService.GetRecipeAsync(recipeId).ConfigureAwait(false);
    }
}
