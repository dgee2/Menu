using AutoFixture.NUnit3;
using FakeItEasy;
using FluentAssertions;
using MenuApi.Controllers;
using MenuApi.Services;
using MenuApi.ViewModel;
using NUnit.Framework;

namespace MenuApi.Tests.Controllers
{
    public class RecipeControllerTests
    {
        private RecipeController sut;
        private IRecipeService recipeService;

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

            var result = await sut.GetRecipesAsync().ConfigureAwait(false);

            result.Should().BeEquivalentTo(recipes);
        }

        [Test, AutoData]
        public async Task GetRecipeAsync_Success(int recipeId, FullRecipe recipe)
        {
            A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

            var result = await sut.GetRecipeAsync(recipeId).ConfigureAwait(false);

            result.Should().Be(recipe);
        }

        [Test, AutoData]
        public async Task GetRecipeIngredientsAsync_Success(int recipeId, IEnumerable<RecipeIngredient> ingredients)
        {
            A.CallTo(() => recipeService.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

            var result = await sut.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);

            result.Should().BeEquivalentTo(ingredients);
        }

        [Test, AutoData]
        public async Task CreateRecipeAsync_Success(NewRecipe newRecipe, FullRecipe recipe, int recipeId)
        {
            A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).Returns(recipeId);
            A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

            var result = await sut.CreateRecipeAsync(newRecipe).ConfigureAwait(false);

            A.CallTo(() => recipeService.CreateRecipeAsync(newRecipe)).MustHaveHappenedOnceExactly();
            result.Should().Be(recipe);
        }

        [Test, AutoData]
        public async Task UpdateRecipeAsync_Success(int recipeId, NewRecipe newRecipe, FullRecipe recipe)
        {
            A.CallTo(() => recipeService.GetRecipeAsync(recipeId)).Returns(recipe);

            var result = await sut.UpdateRecipeAsync(recipeId, newRecipe).ConfigureAwait(false);

            A.CallTo(() => recipeService.UpdateRecipeAsync(recipeId, newRecipe)).MustHaveHappenedOnceExactly();
            result.Should().Be(recipe);
        }
    }
}
