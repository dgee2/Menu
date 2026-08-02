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
    public async Task GetRecipesAsync_Success(IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync()).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService);

        result.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_Success(RecipeId recipeId, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.GetRecipeAsync(recipeService, recipeId);

        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_NotFound_Returns404(RecipeId recipeId)
    {
        RecipeDetail? recipe = null;
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.GetRecipeAsync(recipeService, recipeId);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsAsync_Success(RecipeId recipeId, IEnumerable<RecipeIngredientItem> ingredients)
    {
        A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await RecipeApi.GetRecipeIngredientsAsync(recipeService, recipeId);

        result.Should().BeEquivalentTo(ingredients);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeAsync_Success(UpsertRecipe upsertRecipe, RecipeDetail recipe, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe)).Returns(recipeId);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.CreateRecipeAsync(recipeService, upsertRecipe);

        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_Success(RecipeId recipeId, UpsertRecipe upsertRecipe, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, recipeId, upsertRecipe);

        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }
}
