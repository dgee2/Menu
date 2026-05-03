using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using MenuApi.Repositories;
using MenuApi.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Repositories;

public class RecipeRepositoryTests
{
    [Fact]
    public async Task UpsertRecipeIngredientsAsync_Deduplicates_Exact_Duplicate_New_Links()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedRecipeIngredientGraphAsync(db, cancellationToken);
        var sut = new RecipeRepository(db);

        await sut.UpsertRecipeIngredientsAsync(
            RecipeId.From(1),
            [
                new DBModel.RecipeIngredient(IngredientName.From("Sugar"), IngredientAmount.From(100m), IngredientUnitName.From("Grams")),
                new DBModel.RecipeIngredient(IngredientName.From("Sugar"), IngredientAmount.From(100m), IngredientUnitName.From("Grams")),
            ]);

        var rows = await db.RecipeIngredients
            .Where(ri => ri.RecipeId == 1)
            .ToListAsync(cancellationToken);

        rows.Should().HaveCount(1);
        rows[0].Amount.Should().Be(100m);
    }

    [Fact]
    public async Task UpsertRecipeIngredientsAsync_Does_Not_ReAdd_Existing_Links_On_Update()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var db = CreateDbContext();
        await SeedRecipeIngredientGraphAsync(db, cancellationToken);
        db.RecipeIngredients.Add(new RecipeIngredientEntity
        {
            RecipeId = 1,
            IngredientId = 10,
            UnitId = 4,
            Amount = 50m,
        });
        await db.SaveChangesAsync(cancellationToken);

        var sut = new RecipeRepository(db);

        await sut.UpsertRecipeIngredientsAsync(
            RecipeId.From(1),
            [
                new DBModel.RecipeIngredient(IngredientName.From("Sugar"), IngredientAmount.From(75m), IngredientUnitName.From("Grams")),
                new DBModel.RecipeIngredient(IngredientName.From("Sugar"), IngredientAmount.From(75m), IngredientUnitName.From("Grams")),
            ]);

        var rows = await db.RecipeIngredients
            .Where(ri => ri.RecipeId == 1)
            .ToListAsync(cancellationToken);

        rows.Should().HaveCount(1);
        rows[0].Amount.Should().Be(75m);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MenuDbContext(options);
    }

    private static async Task SeedRecipeIngredientGraphAsync(MenuDbContext db, CancellationToken cancellationToken)
    {
        var unitType = new UnitTypeEntity { Id = 1, Name = "Weight" };
        var unit = new UnitEntity { Id = 4, Name = "Grams", UnitTypeId = unitType.Id, UnitType = unitType };
        var ingredient = new IngredientEntity { Id = 10, Name = "Sugar" };
        var recipe = new RecipeEntity { Id = 1, Name = "Cake" };

        db.UnitTypes.Add(unitType);
        db.Units.Add(unit);
        db.Ingredients.Add(ingredient);
        db.Recipes.Add(recipe);

        await db.SaveChangesAsync(cancellationToken);
    }
}
