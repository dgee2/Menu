using AwesomeAssertions;
using MenuDB;
using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace MenuDB.Tests;

public class RecipeSearchIndexEntityConfigurationTests
{
    [Fact]
    public void RecipeSearchIndex_Maps_Columns_And_Cascades_From_Recipe()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(RecipeSearchIndexEntity))!;

        entityType.GetTableName().Should().Be("RecipeSearchIndex");
        entityType.FindPrimaryKey()!.Properties.Select(property => property.Name).Should().ContainSingle()
            .Which.Should().Be(nameof(RecipeSearchIndexEntity.RecipeId));

        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.Title), "nvarchar(200)", false);
        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.SummaryText), "nvarchar(max)", true);
        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.IngredientsText), "nvarchar(max)", true);
        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.StepsText), "nvarchar(max)", true);
        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.AccessScope), "nvarchar(30)", false);
        AssertColumn(entityType, nameof(RecipeSearchIndexEntity.UpdatedAtUtc), "datetime2", false);

        entityType.FindProperty(nameof(RecipeSearchIndexEntity.IsPublishedExternally))!
            .GetDefaultValue().Should().Be(false);

        var recipeForeignKey = entityType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(RecipeEntity));
        recipeForeignKey.PrincipalEntityType.ClrType.Should().Be(typeof(RecipeEntity));
        recipeForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

        var ownerForeignKey = entityType.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(MenuUserEntity));
        ownerForeignKey.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    private static void AssertColumn(IReadOnlyEntityType entityType, string propertyName, string columnType, bool nullable)
    {
        var property = entityType.FindProperty(propertyName)!;
        property.IsNullable.Should().Be(nullable);
        property.FindAnnotation("Relational:ColumnType")?.Value?.ToString().Should().Be(columnType);
    }

    private static MenuDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MenuDbContext(options);
    }
}
