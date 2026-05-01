#nullable enable

using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Exceptions;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Services;

public class IngredientServiceTests
{
    private readonly IngredientService sut;
    private readonly IIngredientRepository ingredientRepository;
    private readonly IUnitRepository unitRepository;

    public IngredientServiceTests()
    {
        ingredientRepository = A.Fake<IIngredientRepository>();
        unitRepository = A.Fake<IUnitRepository>();
        sut = new IngredientService(unitRepository, ingredientRepository);
    }

    [Theory, CustomAutoData]
    public async Task CreateIngredient_NoExisting_CallsRepository(NewIngredient newIngredient, Ingredient created)
    {
        A.CallTo(() => ingredientRepository.FindByNameAsync(newIngredient.Name))
            .Returns(default(ExistingIngredientLookup?));
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(newIngredient))
            .Returns(created);

        var result = await sut.CreateIngredientAsync(newIngredient);

        result.Should().Be(created);
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(newIngredient)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateIngredient_SameNameSameUnits_ReusesExisting()
    {
        var name = IngredientName.From("Flour");
        var unitIds = new HashSet<int> { 1, 4 };
        var existingIngredient = new Ingredient
        {
            Id = IngredientId.From(42),
            Name = name,
            Units = [],
        };
        var lookup = new ExistingIngredientLookup(existingIngredient, unitIds);

        A.CallTo(() => ingredientRepository.FindByNameAsync(name)).Returns(lookup);

        var newIngredient = new NewIngredient { Name = name, UnitIds = [1, 4] };

        var result = await sut.CreateIngredientAsync(newIngredient);

        result.Should().Be(existingIngredient);
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(A<NewIngredient>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateIngredient_SameNameSameUnits_DifferentOrder_ReusesExisting()
    {
        var name = IngredientName.From("Flour");
        var existingIngredient = new Ingredient
        {
            Id = IngredientId.From(42),
            Name = name,
            Units = [],
        };
        var lookup = new ExistingIngredientLookup(existingIngredient, new HashSet<int> { 1, 4 });

        A.CallTo(() => ingredientRepository.FindByNameAsync(name)).Returns(lookup);

        var newIngredient = new NewIngredient { Name = name, UnitIds = [4, 1] };

        var result = await sut.CreateIngredientAsync(newIngredient);

        result.Should().Be(existingIngredient);
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(A<NewIngredient>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateIngredient_SameNameSameUnitsWithDuplicates_ReusesExisting()
    {
        var name = IngredientName.From("Flour");
        var existingIngredient = new Ingredient
        {
            Id = IngredientId.From(42),
            Name = name,
            Units = [],
        };
        var lookup = new ExistingIngredientLookup(existingIngredient, new HashSet<int> { 1, 4 });

        A.CallTo(() => ingredientRepository.FindByNameAsync(name)).Returns(lookup);

        // Duplicate UnitId 1 in the incoming request - should still match
        var newIngredient = new NewIngredient { Name = name, UnitIds = [1, 1, 4] };

        var result = await sut.CreateIngredientAsync(newIngredient);

        result.Should().Be(existingIngredient);
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(A<NewIngredient>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateIngredient_SameNameDifferentUnits_ThrowsConflictException()
    {
        var name = IngredientName.From("Flour");
        var existingIngredient = new Ingredient
        {
            Id = IngredientId.From(42),
            Name = name,
            Units = [],
        };
        var lookup = new ExistingIngredientLookup(existingIngredient, new HashSet<int> { 1 });

        A.CallTo(() => ingredientRepository.FindByNameAsync(name)).Returns(lookup);

        var newIngredient = new NewIngredient { Name = name, UnitIds = [4] };

        Func<Task> act = () => sut.CreateIngredientAsync(newIngredient);

        await act.Should().ThrowAsync<ConflictException>();
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(A<NewIngredient>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateIngredient_SameNameSubsetOfUnits_ThrowsConflictException()
    {
        var name = IngredientName.From("Flour");
        var existingIngredient = new Ingredient
        {
            Id = IngredientId.From(42),
            Name = name,
            Units = [],
        };
        var lookup = new ExistingIngredientLookup(existingIngredient, new HashSet<int> { 1, 4 });

        A.CallTo(() => ingredientRepository.FindByNameAsync(name)).Returns(lookup);

        // Requesting only unit 1, but existing has {1, 4}
        var newIngredient = new NewIngredient { Name = name, UnitIds = [1] };

        Func<Task> act = () => sut.CreateIngredientAsync(newIngredient);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task CreateIngredient_Null_ThrowsArgumentNullException()
    {
        Func<Task> act = () => sut.CreateIngredientAsync(null!);

        var result = await act.Should().ThrowAsync<ArgumentNullException>();
        result.And.ParamName.Should().Be("newIngredient");
    }
}
