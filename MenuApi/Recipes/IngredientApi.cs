using MenuApi.Repositories;

namespace MenuApi.Recipes;

public static class IngredientApi
{
    public static RouteGroupBuilder MapIngredients(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredient");

        group.WithTags("Ingredients");

        group.MapGet("/", GetIngredients);

        return group;
    }

    public static async Task<IEnumerable<ViewModel.Ingredient>> GetIngredients(IIngredientRepository ingredientRepository)
    {
        return await ingredientRepository.GetIngredientsAsync();
    }
}
