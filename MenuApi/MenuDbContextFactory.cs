using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MenuApi;

/// <summary>Used by EF Core tooling (migrations, scaffolding) and the OpenAPI build-time generator.</summary>
internal sealed class MenuDbContextFactory : IDesignTimeDbContextFactory<MenuDbContext>
{
    public MenuDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseSqlServer("Server=localhost;Database=menu;Trusted_Connection=True;")
            .Options;

        return new MenuDbContext(options);
    }
}
