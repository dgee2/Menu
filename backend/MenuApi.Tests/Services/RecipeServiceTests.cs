using AwesomeAssertions;
using FakeItEasy;
using MenuDB;
using MenuApi.Exceptions;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MenuApi.Tests;

public class RecipeServiceTests
{
    private readonly RecipeService sut;
    private readonly IRecipeRepository recipeRepository;
    private readonly IRecipeStepRepository recipeStepRepository;
    private readonly MenuDbContext db;

    public RecipeServiceTests()
    {
        recipeRepository = A.Fake<IRecipeRepository>();
        recipeStepRepository = A.Fake<IRecipeStepRepository>();

        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        db = new MenuDbContext(options);

        sut = new RecipeService(recipeRepository, recipeStepRepository, db, NullLogger<RecipeService>.Instance);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeSuccess(
        MenuUserId callerId,
        DBModel.Recipe recipe,
        IEnumerable<DBModel.RecipeIngredient> ingredients,
        IEnumerable<DBModel.RecipeStep> steps)
    {
        A.CallTo(() => recipeRepository.GetReadableRecipeAsync(recipe.Id, callerId)).Returns(recipe);
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id, callerId)).Returns(ingredients);
        A.CallTo(() => recipeStepRepository.GetStepsByRecipeIdAsync(recipe.Id)).Returns(steps);

        var expectedIngredients = ingredients.Select(x => new RecipeIngredientItem
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

        var expectedSteps = steps.Select(x => new RecipeStepItem
        {
            SortOrder = x.SortOrder,
            InstructionText = x.InstructionText,
            Title = x.Title,
            DurationMinutes = x.DurationMinutes,
        });

        var result = await sut.GetRecipeAsync(recipe.Id, callerId);

        result!.Title.Should().Be(recipe.Title);
        result.Id.Should().Be(recipe.Id);
        result.AccessScope.Should().Be(recipe.AccessScope);
        result.Summary.Should().Be(recipe.Summary);
        result.Servings.Should().Be(recipe.Servings);
        result.Ingredients.Should().BeEquivalentTo(expectedIngredients);
        result.Steps.Should().BeEquivalentTo(expectedSteps);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipe_NotReadableByCaller_ReturnsNull(RecipeId recipeId, MenuUserId callerId)
    {
        // The cast is load-bearing: a bare null is ambiguous between FakeItEasy's two Returns
        // overloads (CS0121), since the call returns Task<DBModel.Recipe?>.
        A.CallTo(() => recipeRepository.GetReadableRecipeAsync(recipeId, callerId)).Returns((DBModel.Recipe?)null);

        var result = await sut.GetRecipeAsync(recipeId, callerId);

        result.Should().BeNull();
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId, callerId)).MustNotHaveHappened();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipe_OwnedByCaller_CanEditAndDelete(
        MenuUserId callerId,
        DBModel.Recipe recipe,
        IEnumerable<DBModel.RecipeIngredient> ingredients,
        IEnumerable<DBModel.RecipeStep> steps)
    {
        recipe = recipe with { OwnerUserId = callerId };
        A.CallTo(() => recipeRepository.GetReadableRecipeAsync(recipe.Id, callerId)).Returns(recipe);
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id, callerId)).Returns(ingredients);
        A.CallTo(() => recipeStepRepository.GetStepsByRecipeIdAsync(recipe.Id)).Returns(steps);

        var result = await sut.GetRecipeAsync(recipe.Id, callerId);

        result!.CanEdit.Should().BeTrue();
        result.CanDelete.Should().BeTrue();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipe_ReadableButNotOwned_CannotEditOrDelete(
        MenuUserId callerId,
        MenuUserId ownerId,
        DBModel.Recipe recipe,
        IEnumerable<DBModel.RecipeIngredient> ingredients,
        IEnumerable<DBModel.RecipeStep> steps)
    {
        // A shared recipe the caller can read but must not change - the flags are what stops the
        // client from offering an edit button that would only ever 403.
        recipe = recipe with { OwnerUserId = ownerId };
        A.CallTo(() => recipeRepository.GetReadableRecipeAsync(recipe.Id, callerId)).Returns(recipe);
        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipe.Id, callerId)).Returns(ingredients);
        A.CallTo(() => recipeStepRepository.GetStepsByRecipeIdAsync(recipe.Id)).Returns(steps);

        var result = await sut.GetRecipeAsync(recipe.Id, callerId);

        result!.CanEdit.Should().BeFalse();
        result.CanDelete.Should().BeFalse();
    }

    [Theory, CustomAutoData]
    public async Task GetRecipesSuccess(RecipeListScope scope, MenuUserId callerId, IEnumerable<DBModel.Recipe> recipes)
    {
        var expected = recipes.Select(x => new RecipeListItem
        {
            Id = x.Id,
            Title = x.Title,
            AccessScope = x.AccessScope,
            Summary = x.Summary,
            Servings = x.Servings,
            YieldText = x.YieldText,
            PrepTimeMinutes = x.PrepTimeMinutes,
            CookTimeMinutes = x.CookTimeMinutes,
            TotalTimeMinutes = x.TotalTimeMinutes,
        });
        A.CallTo(() => recipeRepository.GetRecipesAsync(scope, callerId, 50)).Returns(recipes.AsEnumerable());

        var result = await sut.GetRecipesAsync(scope, callerId, 50);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task GetRecipeIngredientsSuccess(RecipeId recipeId, MenuUserId callerId, IEnumerable<DBModel.RecipeIngredient> ingredients)
    {
        var expected = ingredients.Select(x => new RecipeIngredientItem
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

        A.CallTo(() => recipeRepository.GetRecipeIngredientsAsync(recipeId, callerId)).Returns(ingredients);

        var result = await sut.GetRecipeIngredientsAsync(recipeId, callerId);
        result.Should().BeEquivalentTo(expected);
    }

    [Theory, CustomAutoData]
    public async Task CreateRecipeSuccess(RecipeId recipeId, MenuUserId callerId, UpsertRecipe upsertRecipe)
    {
        A.CallTo(() => recipeRepository.CreateRecipeAsync(A<DBModel.Recipe>._)).Returns(recipeId);

        await sut.CreateRecipeAsync(upsertRecipe, callerId);

        A.CallTo(() => recipeRepository.CreateRecipeAsync(A<DBModel.Recipe>.That.Matches(
            r => r.Title == upsertRecipe.Title && r.AccessScope == upsertRecipe.AccessScope && r.OwnerUserId == callerId)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeStepRepository.UpsertStepCollectionAsync(recipeId, A<IEnumerable<DBModel.RecipeStep>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipeSuccess(RecipeId recipeId, MenuUserId callerId, UpsertRecipe upsertRecipe, DBModel.Recipe existingRecipe)
    {
        existingRecipe = existingRecipe with { OwnerUserId = callerId };
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns(existingRecipe);

        var result = await sut.UpdateRecipeAsync(recipeId, upsertRecipe, callerId);

        result.Should().BeTrue();
        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, A<DBModel.Recipe>.That.Matches(
            r => r.Title == upsertRecipe.Title && r.AccessScope == upsertRecipe.AccessScope)))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeRepository.UpsertRecipeIngredientsAsync(recipeId, A<IEnumerable<DBModel.RecipeIngredient>>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => recipeStepRepository.UpsertStepCollectionAsync(recipeId, A<IEnumerable<DBModel.RecipeStep>>._)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_RecipeNotFound_ReturnsFalse(RecipeId recipeId, MenuUserId callerId, UpsertRecipe upsertRecipe)
    {
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns((DBModel.Recipe?)null);

        var result = await sut.UpdateRecipeAsync(recipeId, upsertRecipe, callerId);

        result.Should().BeFalse();
        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, A<DBModel.Recipe>._)).MustNotHaveHappened();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_CallerIsNotOwner_ThrowsForbiddenAccessException(
        RecipeId recipeId, MenuUserId callerId, MenuUserId ownerId, UpsertRecipe upsertRecipe, DBModel.Recipe existingRecipe)
    {
        existingRecipe = existingRecipe with { OwnerUserId = ownerId };
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns(existingRecipe);

        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, upsertRecipe, callerId);

        await fun.Should().ThrowAsync<ForbiddenAccessException>();
        A.CallTo(() => recipeRepository.UpdateRecipeAsync(recipeId, A<DBModel.Recipe>._)).MustNotHaveHappened();
    }

    [Theory, CustomAutoData]
    public async Task UpdateRecipe_Should_Throw_Exception_For_null_upsertRecipeAsync(RecipeId recipeId, MenuUserId callerId)
    {
        Func<Task> fun = () => sut.UpdateRecipeAsync(recipeId, null!, callerId);

        var result = await fun.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("upsertRecipe");
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipeSuccess(RecipeId recipeId, MenuUserId callerId, DBModel.Recipe existingRecipe)
    {
        existingRecipe = existingRecipe with { OwnerUserId = callerId };
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns(existingRecipe);

        var result = await sut.DeleteRecipeAsync(recipeId, callerId);

        result.Should().BeTrue();
        A.CallTo(() => recipeRepository.DeleteRecipeAsync(recipeId)).MustHaveHappenedOnceExactly();
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipe_RecipeNotFound_ReturnsFalse(RecipeId recipeId, MenuUserId callerId)
    {
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns((DBModel.Recipe?)null);

        var result = await sut.DeleteRecipeAsync(recipeId, callerId);

        result.Should().BeFalse();
        A.CallTo(() => recipeRepository.DeleteRecipeAsync(recipeId)).MustNotHaveHappened();
    }

    [Theory, CustomAutoData]
    public async Task DeleteRecipe_CallerIsNotOwner_ThrowsForbiddenAccessException(
        RecipeId recipeId, MenuUserId callerId, MenuUserId ownerId, DBModel.Recipe existingRecipe)
    {
        existingRecipe = existingRecipe with { OwnerUserId = ownerId };
        A.CallTo(() => recipeRepository.GetRecipeAsync(recipeId)).Returns(existingRecipe);

        Func<Task> fun = () => sut.DeleteRecipeAsync(recipeId, callerId);

        await fun.Should().ThrowAsync<ForbiddenAccessException>();
        A.CallTo(() => recipeRepository.DeleteRecipeAsync(recipeId)).MustNotHaveHappened();
    }
}
