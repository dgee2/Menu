#nullable enable

using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Recipes;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace MenuApi.Tests.Controllers;

public class RecipeApiTests
{
    private readonly IRecipeService recipeService;

    public RecipeApiTests()
    {
        recipeService = A.Fake<IRecipeService>();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_Success(IEnumerable<Recipe> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync()).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService);

        result.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_Success(RecipeId recipeId, FullRecipe recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.GetRecipeAsync(recipeService, recipeId);

        var okResult = result.Should().BeOfType<Ok<FullRecipe>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_NotFound_Returns404(RecipeId recipeId)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns((FullRecipe?)null);

        var result = await RecipeApi.GetRecipeAsync(recipeService, recipeId);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsAsync_Success(RecipeId recipeId, IEnumerable<RecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await RecipeApi.GetRecipeIngredientsAsync(recipeService, recipeId);

        result.Should().BeEquivalentTo(ingredients);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeAsync_Success(NewRecipe newRecipe, FullRecipe recipe, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).Returns(recipeId);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.CreateRecipeAsync(recipeService, newRecipe);

        A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_Success(RecipeId recipeId, NewRecipe newRecipe, FullRecipe recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, recipeId, newRecipe);

        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, newRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }
}
