using AwesomeAssertions;
using FakeItEasy;
using MenuDB;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests;

public class RecipeServiceTests
{
    private readonly RecipeService sut;
    private readonly IRecipeRepository recipeRepository;
    private readonly MenuDbContext db;

    public RecipeServiceTests()
    {
        recipeRepository = A.Fake<IRecipeRepository>();

        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        db = new MenuDbContext(options);

        sut = new RecipeService(recipeRepository, db);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeSuccess(DBModel.Recipe recipe, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipe.Id)).Returns(recipe);
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id)).Returns(ingredients);

        var expected = ingredients.Select(x => new RecipeIngredient
        {
            SortOrder = x.SortOrder,
            IngredientText = x.IngredientText,
            MeasureText = x.MeasureText,
            SectionTitle = x.SectionTitle,
            Amount = x.Amount,
            UnitText = x.UnitText,
            PreparationText = x.PreparationText,
            IsOptional = x.IsOptional,
            CanonicalIngredientId = x.CanonicalIngredientId,
            CanonicalUnitId = x.CanonicalUnitId,
        });

        var result = await sut.GetRecipeAsync(recipe.Id);

        result!.Title.Should().Be(recipe.Title);
        result.Id.Should().Be(recipe.Id);
        result.Ingredients.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesSuccess(IEnumerable<DBModel.Recipe> recipes)
    {
        var expected = recipes.Select(x => new Recipe
        {
            Id = x.Id,
            Title = x.Title
        });
        A.CallTo(() => recipeRepository.GetRecipesAsync()).Returns(recipes.AsEnumerable());

        var result = await sut.GetRecipesAsync();
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsSuccess(RecipeId recipeId, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        var expected = ingredients.Select(x => new RecipeIngredient
        {
            SortOrder = x.SortOrder,
            IngredientText = x.IngredientText,
            MeasureText = x.MeasureText,
            SectionTitle = x.SectionTitle,
            Amount = x.Amount,
            UnitText = x.UnitText,
            PreparationText = x.PreparationText,
            IsOptional = x.IsOptional,
            CanonicalIngredientId = x.CanonicalIngredientId,
            CanonicalUnitId = x.CanonicalUnitId,
        });

        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId)).Returns(ingredients);

        var result = await sut.GetRecipeIngredientsAsync(recipeId);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeSuccess(DBModel.Recipe recipe, IEnumerable<RecipeIngredient> ingredients)
    {
        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Title)).Returns(recipe.Id);

        var newRecipe = new NewRecipe
        {
            Title = recipe.Title,
            Ingredients = ingredients.ToList(),
        };

        await sut.CreateRecipeAsync(newRecipe);

        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Title)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipe.Id, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeSuccess(RecipeId recipeId, RecipeTitle recipeTitle, IEnumerable<RecipeIngredient> ingredients)
    {
        var newRecipe = new NewRecipe
        {
            Title = recipeTitle,
            Ingredients = ingredients.ToList(),
        };

        await sut.UpdateRecipeAsync(recipeId, newRecipe);

        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, recipeTitle)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_Should_Throw_Exception_For_null_newRecipeAsync(RecipeId recipeId)
    {
        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null!);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newRecipe");
    }
}
