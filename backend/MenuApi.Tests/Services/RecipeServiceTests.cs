using AwesomeAssertions;
using FakeItEasy;
using MenuDB;
using MenuApi.Exceptions;
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
        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Title)).Returns(recipe.Id);

        var newRecipe = new NewRecipe
        {
            Title = recipe.Title,
            Ingredients = ingredients.Select(x => new RecipeIngredient
            {
                Amount = x.Amount,
                Name = x.IngredientName,
                Unit = x.UnitName
            }).ToList()
        };

        await sut.CreateRecipeAsync(newRecipe);

        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipe.Title)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipe.Id, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateRecipeAsync_Deduplicates_Exact_Duplicate_Ingredients_Before_Upsert()
    {
        var recipeId = RecipeId.From(1);
        var recipeTitle = RecipeTitle.From("Cake");
        var duplicateIngredient = new RecipeIngredient
        {
            Name = IngredientName.From("Sugar"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(100m),
        };

        A.CallTo(() => recipeRepository.CreateRecipeAsync(recipeTitle)).Returns(recipeId);

        await sut.CreateRecipeAsync(new NewRecipe
        {
            Title = recipeTitle,
            Ingredients = [duplicateIngredient, duplicateIngredient],
        });

        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(
                recipeId,
                A<IEnumerable<DBModel.RecipeIngredient>>.That.Matches(ingredients =>
                    ingredients.Count() == 1 &&
                    ingredients.Single() == new DBModel.RecipeIngredient(
                        IngredientName.From("Sugar"),
                        IngredientAmount.From(100m),
                        IngredientUnitName.From("Grams")))))
            .MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeSuccess(RecipeId recipeId, RecipeTitle recipeTitle, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        var newRecipe = new NewRecipe
        {
            Title = recipeTitle,
            Ingredients = ingredients.Select(x => new RecipeIngredient
            {
                Amount = x.Amount,
                Name = x.IngredientName,
                Unit = x.UnitName
            }).ToList()
        };

        await sut.UpdateRecipeAsync(recipeId, newRecipe);

        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, recipeTitle)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateRecipeAsync_Deduplicates_Exact_Duplicate_Ingredients_Before_Upsert()
    {
        var recipeId = RecipeId.From(1);
        var recipeTitle = RecipeTitle.From("Cake");
        var duplicateIngredient = new RecipeIngredient
        {
            Name = IngredientName.From("Sugar"),
            Unit = IngredientUnitName.From("Grams"),
            Amount = IngredientAmount.From(100m),
        };

        await sut.UpdateRecipeAsync(recipeId, new NewRecipe
        {
            Title = recipeTitle,
            Ingredients = [duplicateIngredient, duplicateIngredient],
        });

        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(
                recipeId,
                A<IEnumerable<DBModel.RecipeIngredient>>.That.Matches(ingredients =>
                    ingredients.Count() == 1 &&
                    ingredients.Single() == new DBModel.RecipeIngredient(
                        IngredientName.From("Sugar"),
                        IngredientAmount.From(100m),
                        IngredientUnitName.From("Grams")))))
            .MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_Should_Throw_Exception_For_null_newRecipeAsync(RecipeId recipeId)
    {
        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null!);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newRecipe");
    }

    [Fact]
    public async Task CreateRecipeAsync_Throws_BusinessValidationException_When_Same_IngredientUnit_Has_Conflicting_Amounts()
    {
        var recipeTitle = RecipeTitle.From("Cake");

        await sut.Invoking(s => s.CreateRecipeAsync(new NewRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(200m) },
            ],
        })).Should().ThrowAsync<BusinessValidationException>();

        A.CallTo(() => recipeRepository.CreateRecipeAsync(A<RecipeTitle>._)).MustNotHaveHappened();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(A<RecipeId>._, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateRecipeAsync_Throws_BusinessValidationException_When_Same_IngredientUnit_Has_Conflicting_Amounts()
    {
        var recipeId = RecipeId.From(1);
        var recipeTitle = RecipeTitle.From("Cake");

        await sut.Invoking(s => s.UpdateRecipeAsync(recipeId, new NewRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(200m) },
            ],
        })).Should().ThrowAsync<BusinessValidationException>();

        A.CallTo(() => recipeRepository.UpdateRecipeAsync(A<RecipeId>._, A<RecipeTitle>._)).MustNotHaveHappened();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(A<RecipeId>._, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateRecipeAsync_Reports_All_Conflicting_IngredientUnit_Pairs()
    {
        var recipeTitle = RecipeTitle.From("Cake");

        var ex = await sut.Invoking(s => s.CreateRecipeAsync(new NewRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(200m) },
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(300m) },
                new RecipeIngredient { Name = IngredientName.From("Flour"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(400m) },
            ],
        })).Should().ThrowAsync<BusinessValidationException>();

        ex.And.Message.Should().Contain("Sugar");
        ex.And.Message.Should().Contain("Flour");
    }

    [Fact]
    public async Task CreateRecipeAsync_Exact_Duplicate_Then_Conflicting_Detects_Conflict()
    {
        // Three entries: two exact duplicates (silently absorbed) + one with different amount = conflict
        var recipeTitle = RecipeTitle.From("Cake");

        await sut.Invoking(s => s.CreateRecipeAsync(new NewRecipe
        {
            Title = recipeTitle,
            Ingredients =
            [
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(100m) },
                new RecipeIngredient { Name = IngredientName.From("Sugar"), Unit = IngredientUnitName.From("Grams"), Amount = IngredientAmount.From(200m) },
            ],
        })).Should().ThrowAsync<BusinessValidationException>();
    }
}
