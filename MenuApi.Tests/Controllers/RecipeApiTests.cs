using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using MenuApi.Recipes;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Controllers;

public class RecipeApiTests
{
    private readonly IRecipeService recipeService;

    public RecipeApiTests()
    {
        recipeService = A.Fake<IRecipeService>();
    }

    [Theory, AutoData]
    public async Task GetRecipesAsync_Success(IEnumerable<Recipe> recipes)
    {
        A.CallTo(() => recipeService.GetRecipesAsync()).Returns(recipes);

        var result = await RecipeApi.GetRecipes(recipeService);

        result.Should().BeEquivalentTo(recipes);
    }

    [Theory, AutoData]
    public async Task GetRecipeAsync_Success(RecipeId recipeId, FullRecipe recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.GetRecipe(recipeService, recipeId);

        result.Should().Be(recipe);
    }

    [Theory, AutoData]
    public async Task GetRecipeIngredientsAsync_Success(RecipeId recipeId, IEnumerable<RecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await RecipeApi.GetRecipeIngredients(recipeService, recipeId);

        result.Should().BeEquivalentTo(ingredients);
    }

    [Theory, AutoData]
    public async Task CreateRecipeAsync_Success(NewRecipe newRecipe, FullRecipe recipe, RecipeId recipeId)
    {
        A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).Returns(recipeId);
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.CreateRecipe(recipeService, newRecipe);

        A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }

    [Theory, AutoData]
    public async Task UpdateRecipeAsync_Success(RecipeId recipeId, NewRecipe newRecipe, FullRecipe recipe)
    {
        A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

        var result = await RecipeApi.UpdateRecipe(recipeService, recipeId, newRecipe);

        A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, newRecipe)).MustHaveHappenedOnceExactly();
        result.Should().Be(recipe);
    }
}
