﻿using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.Validation;
using MenuApi.ViewModel;

namespace MenuApi.Recipes;

public static class IngredientApi
{
    public static RouteGroupBuilder MapIngredients(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/ingredient");

        group.WithTags("Ingredients");

        group.MapGet("/", GetIngredientsAsync)
            .WithName("getIngredients")
            .Produces<IEnumerable<ViewModel.Ingredient>>(StatusCodes.Status200OK);

        group.MapGet("/unit", GetIngredientUnitsAsync)
            .WithName("getIngredientUnits")
            .Produces<IEnumerable<ViewModel.IngredientUnit>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateIngredientAsync)
            .WithName("createIngredient")
            .AddEndpointFilter<ValidationFilter<NewIngredient>>()
            .Produces<ViewModel.Ingredient>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

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

    public static async Task<ViewModel.Ingredient> CreateIngredientAsync(IIngredientService ingredientService, NewIngredient newIngredient)
    {
        return await ingredientService.CreateIngredientAsync(newIngredient);
    }
}
