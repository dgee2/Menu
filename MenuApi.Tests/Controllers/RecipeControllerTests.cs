using AutoFixture.NUnit3;
using FakeItEasy;
using FluentAssertions;
using MenuApi.Controllers;
using MenuApi.Services;
using MenuApi.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MenuApi.Tests.Controllers
{
    class RecipeControllerTests
    {
        RecipeController sut;
        IRecipeService recipeService;

        [SetUp]
        public void Setup()
        {
            recipeService = A.Fake<IRecipeService>();

            sut = new RecipeController(recipeService);
        }

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_recipeRepository()
        {
            Func<RecipeController> fun = () => new RecipeController(null);
            fun.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("recipeService");
        }

        [Test, AutoData]
        public async Task GetRecipesAsync_Success(IEnumerable<Recipe> recipes)
        {
            A.CallTo(() => recipeService.GetRecipesAsync()).Returns(recipes);

            var result = await sut.GetRecipesAsync();

            result.Should().BeEquivalentTo(recipes);
        }

        [Test, AutoData]
        public async Task GetRecipeAsync_Success(int recipeId, FullRecipe recipe)
        {
            A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

            var result = await sut.GetRecipeAsync(recipeId);

            result.Should().Be(recipe);
        }

        [Test, AutoData]
        public async Task GetRecipeIngredientsAsync_Success(int recipeId, IEnumerable<RecipeIngredient> ingredients)
        {
            A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

            var result = await sut.GetRecipeIngredientsAsync(recipeId);

            result.Should().BeEquivalentTo(ingredients);
        }

        [Test, AutoData]
        public async Task t(NewRecipe newRecipe, FullRecipe recipe, int recipeId)
        {
            A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).Returns(recipeId);
            A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

            var result = await sut.CreateRecipeAsync(newRecipe);


            A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).MustHaveHappenedOnceExactly();
            result.Should().Be(recipe);
        }
    }
}
