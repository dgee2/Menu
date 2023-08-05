using AutoFixture.Xunit2;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.Tests.Factory;
using MenuApi.ViewModel;
using System.Data;
using Xunit;

namespace MenuApi.Tests;

public class RecipeServiceTests
{
    private RecipeService sut;
    private IMapper mapper;
    private IRecipeRepository recipeRepository;
    private ITransactionFactory transactionFactory;
    private IDbTransaction transaction;

    public RecipeServiceTests()
    {
        mapper = AutoMapperFactory.CreateMapper();
        recipeRepository = A.Fake<IRecipeRepository>();
        transactionFactory = A.Fake<ITransactionFactory>();
        transaction = A.Fake<IDbTransaction>();
        A.CallTo(() => transactionFactory.BeginTransaction()).Returns(transaction);

        sut = new RecipeService(recipeRepository, mapper, transactionFactory);
    }

    [Fact]
    public void Constructor_Should_Throw_Exception_For_null_recipeRepository()
    {
        Func<RecipeService> fun = () => new RecipeService(null, mapper, transactionFactory);
        fun.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("recipeRepository");
    }

    [Fact]
    public void Constructor_Should_Throw_Exception_For_null_mapper()
    {
        Func<RecipeService> fun = () => new RecipeService(recipeRepository, null, transactionFactory);
        fun.Should().Throw<ArgumentNullException>()
           .And.ParamName.Should().Be("mapper");
    }

    [Fact]
    public void Constructor_Should_Throw_Exception_For_null_transactionFactory()
    {
        Func<RecipeService> fun = () => new RecipeService(recipeRepository, mapper, null);
        fun.Should().Throw<ArgumentNullException>()
           .And.ParamName.Should().Be("transactionFactory");
    }

    [Theory, AutoData]
    public async Task GetRecipeSuccess(DBModel.Recipe recipe, IEnumerable<DBModel.GetRecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipe.Id)).Returns(recipe);
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id)).Returns(ingredients);

        var expected = ingredients.Select(x => new RecipeIngredient
        {
            Amount = x.Amount,
            Name = x.IngredientName,
            Unit = x.UnitName
        });

        var result = await sut.GetRecipeAsync(recipe.Id).ConfigureAwait(false);

        result.Name.Should().Be(recipe.Name);
        result.Id.Should().Be(recipe.Id);
        result.Ingredients.Should().BeEquivalentTo(expected);
    }

    [Theory, AutoData]
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

    [Theory, AutoData]
    public async Task GetRecipeIngredientsSuccess(int recipeId, IEnumerable<DBModel.GetRecipeIngredient> ingredients)
    {
        var expected = ingredients.Select(x => new RecipeIngredient
        {
            Amount = x.Amount,
            Name = x.IngredientName,
            Unit = x.UnitName
        });

        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await sut.GetRecipeIngredientsAsync(recipeId).ConfigureAwait(false);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task CreateRecipe_Should_Throw_Exception_For_null_newRecipe()
    {
        Func<Task<int>> fun = () => sut.CreateRecipeAsync(null);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newRecipe");
    }

    [Theory, AutoData]
    public async Task CreateRecipeSuccess(DBModel.Recipe recipe, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Name, transaction)).Returns(recipe.Id);

        var newRecipe = new NewRecipe
        {
            Name = recipe.Name,
            Ingredients = ingredients.Select(x => new RecipeIngredient
            {
                Amount = x.Amount,
                Name = x.IngredientName,
                Unit = x.UnitName
            }).ToList()
        };

        await sut.CreateRecipeAsync(newRecipe).ConfigureAwait(false);

        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Name, transaction)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipe.Id, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoData]
    public async Task UpdateRecipeSuccess(int recipeId, string recipeName, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        var newRecipe = new NewRecipe
        {
            Name = recipeName,
            Ingredients = ingredients.Select(x => new RecipeIngredient
            {
                Amount = x.Amount,
                Name = x.IngredientName,
                Unit = x.UnitName
            }).ToList()
        };

        await sut.UpdateRecipeAsync(recipeId, newRecipe).ConfigureAwait(false);

        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, recipeName, transaction)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
    }

    [Theory, AutoData]
    public async Task UpdateRecipe_Should_Throw_Exception_For_null_newRecipeAsync(int recipeId)
    {
        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newRecipe");
    }
}