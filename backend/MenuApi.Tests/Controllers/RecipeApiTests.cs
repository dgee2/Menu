#nullable enable

using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Middleware;
using MenuApi.Recipes;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.AspNetCore.Http;
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

    private static HttpContext CreateHttpContext(MenuUserId? menuUserId)
    {
        var httpContext = new DefaultHttpContext();
        if (menuUserId is not null)
        {
            httpContext.Items[MenuUserHttpContextKeys.MenuUserId] = menuUserId.Value;
        }

        return httpContext;
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_Mine_Success(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 50)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "mine", null);

        var okResult = result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_Authenticated_Success(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Authenticated, callerId, 50)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "AUTHENTICATED", null);

        var okResult = result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>().Subject;
        okResult.Value.Should().BeEquivalentTo(recipes);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsTakeToMax(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 200)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "mine", 500);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 200)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsZeroTakeToMin(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "mine", 0);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_ClampsNegativeTakeToMin(MenuUserId callerId, IEnumerable<RecipeListItem> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).Returns(recipes);

        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "mine", -1);

        result.Should().BeOfType<Ok<IEnumerable<RecipeListItem>>>();
        A.CallTo(() => recipeService.GetRecipesAsync(RecipeListScope.Mine, callerId, 1)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_UnknownScope_Returns400(MenuUserId callerId)
    {
        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), "everyone", null);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
        problemResult.ProblemDetails.Detail.Should().Be("Unknown scope 'everyone'. Expected 'mine' or 'authenticated'.");
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesAsync_MissingScope_Returns400(MenuUserId callerId)
    {
        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(callerId), null, null);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
        problemResult.ProblemDetails.Detail.Should().Be("Missing scope. Expected 'mine' or 'authenticated'.");
    }

    [Fact]
    public async Task GetRecipesAsync_NoMenuUserId_Returns401()
    {
        var result = await RecipeApi.GetRecipesAsync(recipeService, CreateHttpContext(null), "mine", null);

        result.Should().BeOfType<UnauthorizedHttpResult>();
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
    public async Task CreateRecipeAsync_Success(MenuUserId callerId, UpsertRecipe upsertRecipe, RecipeDetail recipe, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe, callerId)).Returns(recipeId);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.CreateRecipeAsync(recipeService, CreateHttpContext(callerId), upsertRecipe);

        A.CallTo(() => recipeService.CreateRecipeAsync(upsertRecipe, callerId)).MustHaveHappenedOnceExactly();
        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeAsync_NoMenuUserId_Returns401(UpsertRecipe upsertRecipe)
    {
        var result = await RecipeApi.CreateRecipeAsync(recipeService, CreateHttpContext(null), upsertRecipe);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_Success(MenuUserId callerId, RecipeId recipeId, UpsertRecipe upsertRecipe, RecipeDetail recipe)
    {
        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).Returns(true);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, CreateHttpContext(callerId), recipeId, upsertRecipe);

        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).MustHaveHappenedOnceExactly();
        var okResult = result.Should().BeOfType<Ok<RecipeDetail>>().Subject;
        okResult.Value.Should().Be(recipe);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_NotFound_Returns404(MenuUserId callerId, RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, upsertRecipe, callerId)).Returns(false);

        var result = await RecipeApi.UpdateRecipeAsync(recipeService, CreateHttpContext(callerId), recipeId, upsertRecipe);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeAsync_NoMenuUserId_Returns401(RecipeId recipeId, UpsertRecipe upsertRecipe)
    {
        var result = await RecipeApi.UpdateRecipeAsync(recipeService, CreateHttpContext(null), recipeId, upsertRecipe);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeAsync_Success(MenuUserId callerId, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).Returns(true);

        var result = await RecipeApi.DeleteRecipeAsync(recipeService, CreateHttpContext(callerId), recipeId);

        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).MustHaveHappenedOnceExactly();
        result.Should().BeOfType<NoContent>();
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeAsync_NotFound_Returns404(MenuUserId callerId, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.DeleteRecipeAsync(recipeId, callerId)).Returns(false);

        var result = await RecipeApi.DeleteRecipeAsync(recipeService, CreateHttpContext(callerId), recipeId);

        var problemResult = result.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeAsync_NoMenuUserId_Returns401(RecipeId recipeId)
    {
        var result = await RecipeApi.DeleteRecipeAsync(recipeService, CreateHttpContext(null), recipeId);

        result.Should().BeOfType<UnauthorizedHttpResult>();
    }
}
