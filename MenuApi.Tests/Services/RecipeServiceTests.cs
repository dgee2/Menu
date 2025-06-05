using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Factory;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using System.Data;
using Xunit;

namespace MenuApi.Tests;

public class RecipeServiceTests
{
    private readonly RecipeService sut;
    private readonly IRecipeRepository recipeRepository;
    private readonly ITransactionFactory transactionFactory;
    private readonly IDbTransaction transaction;

    public RecipeServiceTests()
    {
        recipeRepository = A.Fake<IRecipeRepository>();
        transactionFactory = A.Fake<ITransactionFactory>();
        transaction = A.Fake<IDbTransaction>();
        A.CallTo(() => transactionFactory.BeginTransaction()).Returns(transaction);

        sut = new RecipeService(recipeRepository, transactionFactory);
    }

    [Theory, CustomAutoData]
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

        var result = await sut.GetRecipeAsync(recipe.Id);

        result.Name.Should().Be(recipe.Name);
        result.Id.Should().Be(recipe.Id);
        result.Ingredients.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesSuccess(IEnumerable<DBModel.Recipe> recipes)
    {
        var expected = recipes.Select(x => new Recipe
        {
            Id = x.Id,
            Name = x.Name
        });
        A.CallTo(() => recipeRepository.GetRecipesAsync()).Returns(recipes.AsEnumerable());

        var result = await sut.GetRecipesAsync();
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsSuccess(RecipeId recipeId, IEnumerable<DBModel.GetRecipeIngredient> ingredients)
    {
        var expected = ingredients.Select(x => new RecipeIngredient
        {
            Amount = x.Amount,
            Name = x.IngredientName,
            Unit = x.UnitName
        });

        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await sut.GetRecipeIngredientsAsync(recipeId);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
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

        await sut.CreateRecipeAsync(newRecipe);

        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Name, transaction)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipe.Id, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeSuccess(RecipeId recipeId, RecipeName recipeName, IEnumerable<DBModel.RecipeIngredient> ingredients)
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

        await sut.UpdateRecipeAsync(recipeId, newRecipe);

        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, recipeName, transaction)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._, transaction)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_Should_Throw_Exception_For_null_newRecipeAsync(RecipeId recipeId)
    {
        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newRecipe");
    }
}