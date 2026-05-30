using MenuDB.Data;
using Microsoft.EntityFrameworkCore;

namespace MenuDB;

public class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<MenuUserEntity> MenuUsers { get; set; }

    public DbSet<RecipeEntity> Recipes { get; set; }
    public DbSet<IngredientEntity> Ingredients { get; set; }
    public DbSet<UnitTypeEntity> UnitTypes { get; set; }
    public DbSet<UnitEntity> Units { get; set; }
    public DbSet<IngredientUnitEntity> IngredientUnits { get; set; }
    public DbSet<RecipeIngredientEntity> RecipeIngredients { get; set; }

    public DbSet<RecipeStepEntity> RecipeSteps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MenuDbContext).Assembly);
    }
}
