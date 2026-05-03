using AwesomeAssertions;
using FakeItEasy;
using MenuApi.Repositories;
using MenuApi.Services;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Xunit;

namespace MenuApi.Tests.Services;

public class IngredientServiceTests
{
    [Fact]
    public async Task CreateIngredientAsync_Deduplicates_UnitIds_Before_Repository_Call()
    {
        var unitRepository = A.Fake<IUnitRepository>();
        var ingredientRepository = A.Fake<IIngredientRepository>();
        var expected = new Ingredient
        {
            Id = IngredientId.From(1),
            Name = IngredientName.From("Sugar"),
            Units = [],
        };

        A.CallTo(() => ingredientRepository.CreateIngredientAsync(
                A<NewIngredient>.That.Matches(i =>
                    i.Name == IngredientName.From("Sugar") &&
                    i.UnitIds.Count == 2 &&
                    i.UnitIds.Contains(1) &&
                    i.UnitIds.Contains(4))))
            .Returns(expected);

        var sut = new IngredientService(unitRepository, ingredientRepository);

        var result = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [1, 4, 1, 4],
        });

        result.Should().Be(expected);
        A.CallTo(() => ingredientRepository.CreateIngredientAsync(
                A<NewIngredient>.That.Matches(i =>
                    i.Name == IngredientName.From("Sugar") &&
                    i.UnitIds.Count == 2 &&
                    i.UnitIds.Contains(1) &&
                    i.UnitIds.Contains(4))))
            .MustHaveHappenedOnceExactly();
    }
}
