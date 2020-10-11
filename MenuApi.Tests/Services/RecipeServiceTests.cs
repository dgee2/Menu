using AutoFixture.NUnit3;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.Tests.Factory;
using MenuApi.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MenuApi.Tests
{
    public class RecipeServiceTests
    {
        private RecipeService sut;
        private IMapper mapper;
        private IRecipeRepository recipeRepository;
        private ITransactionFactory transactionFactory;
        private IDbTransaction transaction;

        [SetUp]
        public void Setup()
        {
            mapper = AutoMapperFactory.CreateMapper();
            recipeRepository = A.Fake<IRecipeRepository>();
            transactionFactory = A.Fake<ITransactionFactory>();
            transaction = A.Fake<IDbTransaction>();
            A.CallTo(() => transactionFactory.BeginTransaction()).Returns(transaction);

            sut = new RecipeService(recipeRepository, mapper, transactionFactory);
        }

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_recipeRepository()
        {
            Func<RecipeService> fun = () => new RecipeService(null, mapper, transactionFactory);
            fun.Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be("recipeRepository");
        }

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_mapper()
        {
            Func<RecipeService> fun = () => new RecipeService(recipeRepository, null, transactionFactory);
            fun.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("mapper");
        }

        [Test]
        public void Constructor_Should_Throw_Exception_For_null_transactionFactory()
        {
            Func<RecipeService> fun = () => new RecipeService(recipeRepository, mapper, null);
            fun.Should().Throw<ArgumentNullException>()
               .And.ParamName.Should().Be("transactionFactory");
        }

        [Test, AutoData]
        public async Task GetRecipeSuccess(DBModel.Recipe recipe, IEnumerable<DBModel.RecipeIngredient> ingredients)
        {
            A.CallTo(() => recipeRepository.GetRecipeAsync(recipe.Id)).Returns(recipe);
            A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id)).Returns(ingredients);

            var result = await sut.GetRecipeAsync(recipe.Id).ConfigureAwait(false);

            result.Name.Should().Be(recipe.Name);
            result.Id.Should().Be(recipe.Id);
            result.Ingredients.Should().BeEquivalentTo(ingredients);
        }

        [Test, AutoData]
        public async Task GetRecipesSuccess(IEnumerable<DBModel.Recipe> recipes)
        {
            var expected = recipes.Select(x => new Recipe
            {
                Id = x.Id,
                Name = x.Name
            });
            A.CallTo(() => recipeRepository.GetRecipesAsync()).Returns(recipes.AsEnumerable());

            var result = await sut.GetRecipesAsync().ConfigureAwait(false);
            result.Should().BeEquivalentTo(expected);
        }

        [Test, AutoData]
        public async Task GetRecipeIngredientsSuccess(int recipeId, IEnumerable<DBModel.RecipeIngredient> ingredients)
        {
            var expected = ingredients.Select(x => new RecipeIngredient
            {
                Amount = x.Amount,
                Name = x.Name,
                Unit = x.Unit
            });

            A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

            var result = await sut.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CreateRecipe_Should_Throw_Exception_For_null_newRecipe()
        {
            Func<Task<int>> fun = () => sut.CreateRecipeAsync(null);

            fun.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("newRecipe");
        }

        [Test, AutoData]
        public async Task CreateRecipeSuccess(DBModel.Recipe recipe, IEnumerable<DBModel.RecipeIngredient> ingredients)
        {
            A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Name, transaction)).Returns(recipe.Id);

            var newRecipe = new NewRecipe
            {
                Name = recipe.Name,
                Ingredients = ingredients.Select(x => new RecipeIngredient
                {
                    Amount = x.Amount,
                    Name = x.Name,
                    Unit = x.Unit
                }).ToList()
            };

            await sut.CreateRecipeAsync(newRecipe).ConfigureAwait(false);

            A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Name, transaction)).MustHaveHappenedOnceExactly();
            A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipe.Id, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
        }

        [Test, AutoData]
        public async Task UpdateRecipeSuccess(int recipeId, string recipeName, IEnumerable<DBModel.RecipeIngredient> ingredients)
        {
            var newRecipe = new NewRecipe
            {
                Name = recipeName,
                Ingredients = ingredients.Select(x => new RecipeIngredient
                {
                    Amount = x.Amount,
                    Name = x.Name,
                    Unit = x.Unit
                }).ToList()
            };

            await sut.UpdateRecipeAsync(recipeId, newRecipe).ConfigureAwait(false);

            A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, recipeName, transaction)).MustHaveHappenedOnceExactly();
            A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
        }

        [Test, AutoData]
        public void UpdateRecipe_Should_Throw_Exception_For_null_newRecipe(int recipeId)
        {
            Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null);

            fun.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("newRecipe");
        }
    }
}