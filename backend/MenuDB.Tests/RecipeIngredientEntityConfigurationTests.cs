using AwesomeAssertions;
using MenuDB;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MenuDB.Tests;

public class RecipeIngredientEntityConfigurationTests
{
    [Fact]
    public void RecipeIngredient_Uses_ApplicationGenerated_Guid_Keys()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(Data.RecipeIngredientEntity))!;

        entityType.FindProperty(nameof(Data.RecipeIngredientEntity.Id))!.ClrType.Should().Be<Guid>();
        entityType.FindProperty(nameof(Data.RecipeIngredientEntity.RecipeId))!.ClrType.Should().Be<Guid>();
        entityType.FindProperty(nameof(Data.RecipeIngredientEntity.Id))!.ValueGenerated
            .Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MenuDbContext(options);
    }
}
