using MenuApi.Repositories;
using MenuApi.Services;

namespace MenuApi.Recipes;

public static class IngredientApi
{
    public static RouteGroupBuilder MapIngredients(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredient");

        group.WithTags("Ingredients");

        group.MapGet("/", GetIngredientsAsync);

        group.MapGet("/unit", GetIngredientUnitsAsync);

        return group;
    }

    public static async Task<IEnumerable<ViewModel.Ingredient>> GetIngredientsAsync(IIngredientRepository ingredientRepository)
    {
        return await ingredientRepository.GetIngredientsAsync();
    }

    public static async Task<IEnumerable<ViewModel.IngredientUnit>> GetIngredientUnitsAsync(IIngredientService ingredientService)
    {
        return await ingredientService.GetIngredientUnitsAsync();
    }
}
