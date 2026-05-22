using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using MenuApi.ViewModel;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

public class IngredientRepositoryTests
{
    [Fact]
    public async Task CreateIngredientAsync_Creates_New_Ingredient_When_Name_Does_Not_Exist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedUnitsAsync(db, cancellationToken);
        var sut = new IngredientRepository(db);

        var result = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [4],
        });

        result.Id.Value.Should().BeGreaterThan(0);
        result.Name.Should().Be(IngredientName.From("Sugar"));
        result.Units.Should().ContainSingle(u => u.Name == IngredientUnitName.From("Grams"));

        var count = await db.Ingredients.CountAsync(cancellationToken);
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateIngredientAsync_Returns_Existing_Ingredient_When_Name_Already_Exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedUnitsAsync(db, cancellationToken);

        var unitType = new UnitTypeEntity { Id = 99, Name = "Other" };
        var otherUnit = new UnitEntity { Id = 99, Name = "Cup", UnitTypeId = 99, UnitType = unitType };
        db.UnitTypes.Add(unitType);
        db.Units.Add(otherUnit);
        await db.SaveChangesAsync(cancellationToken);

        var sut = new IngredientRepository(db);
        var first = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [4],
        });

        var second = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [99],
        });

        second.Id.Should().Be(first.Id);
        second.Units.Should().ContainSingle(u => u.Name == IngredientUnitName.From("Grams"));

        var count = await db.Ingredients.CountAsync(i => i.Name == "Sugar", cancellationToken);
        count.Should().Be(1);
    }

    [Fact]
    public async Task CreateIngredientAsync_Returns_Existing_Ingredient_When_Name_Already_Exists_Even_With_Unknown_UnitId()
    {
        // UnitIds are only used when inserting a new row. When the canonical row already exists,
        // the provided UnitIds are intentionally ignored — the existing ingredient is returned as-is.
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedUnitsAsync(db, cancellationToken);

        var sut = new IngredientRepository(db);
        var first = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [4],
        });

        var second = await sut.CreateIngredientAsync(new NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [9999], // non-existent unit ID — ignored because ingredient already exists
        });

        second.Id.Should().Be(first.Id);
        second.Units.Should().ContainSingle(u => u.Name == IngredientUnitName.From("Grams"));
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MenuDbContext(options);
    }

    private static async Task SeedUnitsAsync(MenuDbContext db, CancellationToken cancellationToken)
    {
        var unitType = new UnitTypeEntity { Id = 3, Name = "Weight" };
        var unit = new UnitEntity { Id = 4, Name = "Grams", UnitTypeId = 3, UnitType = unitType };

        db.UnitTypes.Add(unitType);
        db.Units.Add(unit);

        await db.SaveChangesAsync(cancellationToken);
    }
}
