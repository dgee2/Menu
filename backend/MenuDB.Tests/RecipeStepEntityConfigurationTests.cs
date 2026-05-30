using AwesomeAssertions;
using MenuDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace MenuDB.Tests;

public class RecipeStepEntityConfigurationTests
{
    [Fact]
    public void RecipeStep_Table_Has_Correct_Name()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(Data.RecipeStepEntity));
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("RecipeStep");
    }

    [Fact]
    public void RecipeStep_InstructionText_Is_NvarcharMax_Required()
    {
        using var db = CreateDbContext();
        var property = db.Model.FindEntityType(typeof(Data.RecipeStepEntity))!
            .FindProperty(nameof(Data.RecipeStepEntity.InstructionText))!;
        GetConfiguredColumnType(property).Should().Be("nvarchar(max)");
        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void RecipeStep_Title_Is_Nvarchar200_Nullable()
    {
        using var db = CreateDbContext();
        var property = db.Model.FindEntityType(typeof(Data.RecipeStepEntity))!
            .FindProperty(nameof(Data.RecipeStepEntity.Title))!;
        GetConfiguredColumnType(property).Should().Be("nvarchar(200)");
        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void RecipeStep_DurationMinutes_Is_Nullable()
    {
        using var db = CreateDbContext();
        var property = db.Model.FindEntityType(typeof(Data.RecipeStepEntity))!
            .FindProperty(nameof(Data.RecipeStepEntity.DurationMinutes))!;
        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void RecipeStep_HasCascadeDelete_From_Recipe()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(Data.RecipeStepEntity))!;
        var fk = entityType.GetForeignKeys()
            .Single(fk => fk.GetConstraintName() == "FK_RecipeStep_ToRecipe");
        fk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    private static string GetConfiguredColumnType(IReadOnlyAnnotatable property) =>
        property.FindAnnotation("Relational:ColumnType")?.Value?.ToString() ?? string.Empty;

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MenuDbContext(options);
    }
}
