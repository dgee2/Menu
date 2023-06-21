using MenuApi.Repositories;

namespace MenuApi.Recipes;

public static class IngredientApi
{
    public static RouteGroupBuilder MapIngredients(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredients");

        group.WithTags("Ingredients");

        group.MapGet("/", async (IIngredientRepository ingredientRepository) =>
        {
            return await ingredientRepository.GetIngredientsAsync();
        });

        return group;
    }
}
