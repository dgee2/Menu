using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuApi.Tests.Database;

public class MenuDbContextTests
{
    [Fact]
    public void Ingredient_Name_Has_Unique_Index()
    {
        using var db = CreateDbContext();

        var ingredientEntity = db.Model.FindEntityType(typeof(IngredientEntity));
        var ingredientNameIndex = ingredientEntity!.GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(["Name"]));

        ingredientNameIndex.IsUnique.Should().BeTrue();
    }

    [Fact]
    public void Recipe_Name_Has_Unique_Index()
    {
        using var db = CreateDbContext();

        var recipeEntity = db.Model.FindEntityType(typeof(RecipeEntity));
        var recipeNameIndex = recipeEntity!.GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name).SequenceEqual(["Name"]));

        recipeNameIndex.IsUnique.Should().BeTrue();
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MenuDbContext(options);
    }
}
