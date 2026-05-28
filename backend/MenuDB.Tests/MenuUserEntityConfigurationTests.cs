using AwesomeAssertions;
using MenuDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace MenuDB.Tests;

public class MenuUserEntityConfigurationTests
{
    [Fact]
    public void MenuUser_Table_Has_Correct_Name_And_Schema()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("MenuUser");
        entityType.GetSchema().Should().Be("identity");
    }

    [Fact]
    public void MenuUser_AuthSubject_Is_Nvarchar256_Required()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity))!;
        var property = entityType.FindProperty(nameof(MenuDB.Data.MenuUserEntity.AuthSubject))!;

        GetConfiguredColumnType(property).Should().Be("nvarchar(256)");
        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void MenuUser_DisplayName_Is_Nvarchar100_Required()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity))!;
        var property = entityType.FindProperty(nameof(MenuDB.Data.MenuUserEntity.DisplayName))!;

        GetConfiguredColumnType(property).Should().Be("nvarchar(100)");
        property.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void MenuUser_Email_Is_Nvarchar256_Nullable()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity))!;
        var property = entityType.FindProperty(nameof(MenuDB.Data.MenuUserEntity.Email))!;

        GetConfiguredColumnType(property).Should().Be("nvarchar(256)");
        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void MenuUser_AvatarUrl_Is_Nvarchar512_Nullable()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity))!;
        var property = entityType.FindProperty(nameof(MenuDB.Data.MenuUserEntity.AvatarUrl))!;

        GetConfiguredColumnType(property).Should().Be("nvarchar(512)");
        property.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void MenuUser_Has_Unique_Index_On_AuthSubject()
    {
        using var db = CreateDbContext();

        var entityType = db.Model.FindEntityType(typeof(MenuDB.Data.MenuUserEntity))!;
        var index = entityType.GetIndexes()
            .SingleOrDefault(i => i.GetDatabaseName() == "UX_MenuUser_AuthSubject");

        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.Properties.Should().ContainSingle(p => p.Name == nameof(MenuDB.Data.MenuUserEntity.AuthSubject));
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
