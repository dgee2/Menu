using MenuApi.Repositories;
using MenuApi.Services;

namespace MenuApi.Recipes;

public static class IngredientApi
{
    public static RouteGroupBuilder MapIngredients(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredient");

        group.WithTags("Ingredients");

        group.MapGet("/", GetIngredients);

        group.MapGet("/unit", GetIngredientUnits);

        return group;
    }

    public static async Task<IEnumerable<ViewModel.Ingredient>> GetIngredients(IIngredientRepository ingredientRepository)
    {
        return await ingredientRepository.GetIngredientsAsync();
    }

    public static async Task<IEnumerable<ViewModel.IngredientUnit>> GetIngredientUnits(IIngredientService ingredientService)
    {
        return await ingredientService.GetIngredientUnitsAsync();
    }
}
