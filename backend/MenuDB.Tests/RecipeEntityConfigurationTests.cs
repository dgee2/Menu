using AwesomeAssertions;
using MenuDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace MenuDB.Tests;

public class RecipeEntityConfigurationTests
{
    [Fact]
    public void Recipe_Uses_ApplicationGenerated_Guid_Key_And_SoftDelete_Filter()
    {
        using var db = CreateDbContext();
        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.RecipeEntity))!;
        var id = entityType.FindProperty(nameof(MenuDB.Data.RecipeEntity.Id))!;

        id.ClrType.Should().Be<Guid>();
        id.ValueGenerated.Should().Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
        entityType.GetDeclaredQueryFilters().Should().ContainSingle();

        var index = entityType.GetIndexes()
            .Single(index => index.GetDatabaseName() == "UX_Recipe_OwnerUserId_Title");
        index.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Be("[DeletedAtUtc] IS NULL");
    }

    [Fact]
    public void Recipe_DeletedAtUtc_Is_Nullable()
    {
        using var db = CreateDbContext();
        var property = db.Model.FindEntityType(typeof(MenuDB.Data.RecipeEntity))!
            .FindProperty(nameof(MenuDB.Data.RecipeEntity.DeletedAtUtc))!;

        property.ClrType.Should().Be<DateTime?>();
        property.IsNullable.Should().BeTrue();
        GetConfiguredColumnType(property).Should().Be("datetime2");
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
