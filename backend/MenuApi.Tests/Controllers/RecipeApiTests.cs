#nullable enable

using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Middleware;
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

    // The "no caller" path is RequireCallerFilter's job now, so handlers here are always called
    // with a bound caller - see RequireCallerFilterTests for the 401.
    private static CallerId Caller(MenuUserId menuUserId) => new(menuUserId);

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_Mine_Success(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 50)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "mine", null);

        var okResult = result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_Authenticated_Success(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Authenticated, callerId, 50)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "AUTHENTICATED", null);

        var okResult = result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsTakeToMax(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 200)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "mine", 500);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 200)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsZeroTakeToMin(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "mine", 0);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsNegativeTakeToMin(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "mine", -1);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_UnknownScope_ReturnsValidationProblemKeyedOnScope(MenuUserId callerId)
    {
        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), "everyone", null);

        var problemResult = result.Should().BeOfType<ValidationProblem>().Subject;
        problemResult.StatusCode.Should().Be(400);
        problemResult.ProblemDetails.Errors.Should().ContainKey("scope");
        problemResult.ProblemDetails.Errors["scope"].Should()
            .ContainSingle().Which.Should().Be("Unknown scope 'everyone'. Expected one of: mine, authenticated.");
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_MissingScope_ReturnsValidationProblemKeyedOnScope(MenuUserId callerId)
    {
        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(callerId), null, null);

        var problemResult = result.Should().BeOfType<ValidationProblem>().Subject;
        problemResult.StatusCode.Should().Be(400);
        problemResult.ProblemDetails.Errors["scope"].Should()
            .ContainSingle().Which.Should().Be("Missing scope. Expected one of: mine, authenticated.");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData(" 1")] // Enum.TryParse would trim and accept this
    [InlineData("mine,authenticated")] // Enum.TryParse would OR these together
    [InlineData("mine ")]
    public async Task GetRecipesAsync_NonMemberNameScope_ReturnsValidationProblem(string scope)
    {
        // The enum's ordinals must not become a second, undocumented spelling of the contract.
        var result = await RecipeApi.GetRecipesAsync(recipeService, Caller(MenuUserId.From(1)), scope, null);

        result.Should().BeOfType<ValidationProblem>();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_Success(MenuUserId callerId, RecipeId recipeId, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).Returns(recipe);

        var result = await RecipeApi.GetRecipeAsync(recipeService, Caller(callerId), recipeId);

        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_NotReadable_Returns404(MenuUserId callerId, RecipeId recipeId)
    {
        RecipeDetail? recipe = null;
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).Returns(recipe);

        var result = await RecipeApi.GetRecipeAsync(recipeService, Caller(callerId), recipeId);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeAsync_PassesCallerToService(MenuUserId callerId, RecipeId recipeId, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).Returns(recipe);

        await RecipeApi.GetRecipeAsync(recipeService, Caller(callerId), recipeId);

        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsAsync_Success(MenuUserId callerId, RecipeId recipeId, IEnumerable<RecipeIngredientItem> ingredients)
    {
        A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId, callerId)).Returns(ingredients);

        var result = await RecipeApi.GetRecipeIngredientsAsync(recipeService, Caller(callerId), recipeId);

        result.Should().BeEquivalentTo(ingredients);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeAsync_Success(MenuUserId callerId, UpsertRecipe upsertRecipe, RecipeDetail recipe, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe, callerId)).Returns(recipeId);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).Returns(recipe);

        var result = await RecipeApi.CreateRecipeAsync(recipeService, Caller(callerId), upsertRecipe);

        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe, callerId)).MustHaveHappenedOnceExactly();
        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_Success(MenuUserId callerId, RecipeId recipeId, UpsertRecipe upsertRecipe, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).Returns(true);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId, callerId)).Returns(recipe);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, Caller(callerId), recipeId, upsertRecipe);

        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).MustHaveHappenedOnceExactly();
        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_NotFound_Returns404(MenuUserId callerId, RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).Returns(false);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, Caller(callerId), recipeId, upsertRecipe);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeAsync_Success(MenuUserId callerId, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).Returns(true);

        var result = await RecipeApi.DeleteRecipeAsync(recipeService, Caller(callerId), recipeId);

        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContent>();
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeAsync_NotFound_Returns404(MenuUserId callerId, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).Returns(false);

        var result = await RecipeApi.DeleteRecipeAsync(recipeService, Caller(callerId), recipeId);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }
}
