using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Exceptions;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

public class IngredientRepositoryTests
{
    [Fact]
    public async Task CreateIngredientAsync_Reuses_Equivalent_Canonical_Ingredient()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedUnitGraphAsync(db, cancellationToken);
        db.Ingredients.Add(new IngredientEntity
        {
            Id = 10,
            Name = "Sugar",
            IngredientUnits =
            [
                new IngredientUnitEntity { UnitId = 1 },
                new IngredientUnitEntity { UnitId = 4 },
            ],
        });
        await db.SaveChangesAsync(cancellationToken);

        var sut = new IngredientRepository(db);

        var result = await sut.CreateIngredientAsync(new MenuApi.ViewModel.NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [4, 1, 4],
        });

        result.Id.Should().Be(IngredientId.From(10));
        result.Name.Should().Be(IngredientName.From("Sugar"));
        result.Units.Should().HaveCount(2);
        result.Units.Should().ContainSingle(u => u.Name == IngredientUnitName.From("Millilitres"));
        result.Units.Should().ContainSingle(u => u.Name == IngredientUnitName.From("Grams"));
        (await db.Ingredients.CountAsync(i => i.Name == "Sugar", cancellationToken)).Should().Be(1);
    }

    [Fact]
    public async Task CreateIngredientAsync_Rejects_Canonical_Redefinition()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedUnitGraphAsync(db, cancellationToken);
        db.Ingredients.Add(new IngredientEntity
        {
            Id = 10,
            Name = "Sugar",
            IngredientUnits =
            [
                new IngredientUnitEntity { UnitId = 1 },
            ],
        });
        await db.SaveChangesAsync(cancellationToken);

        var sut = new IngredientRepository(db);

        var act = async () => await sut.CreateIngredientAsync(new MenuApi.ViewModel.NewIngredient
        {
            Name = IngredientName.From("Sugar"),
            UnitIds = [1, 4],
        });

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Ingredient 'Sugar' already exists with a different unit set.");
        (await db.Ingredients.CountAsync(i => i.Name == "Sugar", cancellationToken)).Should().Be(1);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MenuDbContext(options);
    }

    private static async Task SeedUnitGraphAsync(MenuDbContext db, CancellationToken cancellationToken)
    {
        var volumeType = new UnitTypeEntity { Id = 1, Name = "Volume" };
        var weightType = new UnitTypeEntity { Id = 3, Name = "Weight" };

        db.UnitTypes.AddRange(volumeType, weightType);
        db.Units.AddRange(
            new UnitEntity { Id = 1, Name = "Millilitres", Abbreviation = "ml", UnitTypeId = 1, UnitType = volumeType },
            new UnitEntity { Id = 4, Name = "Grams", Abbreviation = "g", UnitTypeId = 3, UnitType = weightType });

        await db.SaveChangesAsync(cancellationToken);
    }
}
