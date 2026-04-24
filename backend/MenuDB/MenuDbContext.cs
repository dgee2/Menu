using MenuDB.Data;
using Microsoft.EntityFrameworkCore;

namespace MenuDB;

public class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<RecipeEntity> Recipes { get; set; }
    public DbSet<IngredientEntity> Ingredients { get; set; }
    public DbSet<UnitTypeEntity> UnitTypes { get; set; }
    public DbSet<UnitEntity> Units { get; set; }
    public DbSet<IngredientUnitEntity> IngredientUnits { get; set; }
    public DbSet<RecipeIngredientEntity> RecipeIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeEntity>(e =>
        {
            e.ToTable("Recipe");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.Name).HasColumnType("varchar(500)").IsRequired();
        });

        modelBuilder.Entity<IngredientEntity>(e =>
        {
            e.ToTable("Ingredient");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).UseIdentityColumn();
            e.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Ingredient_Name");
        });

        modelBuilder.Entity<UnitTypeEntity>(e =>
        {
            e.ToTable("UnitType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
            e.HasData(
                new UnitTypeEntity { Id = 1, Name = "Volume" },
                new UnitTypeEntity { Id = 2, Name = "Quantity" },
                new UnitTypeEntity { Id = 3, Name = "Weight" });
        });

        modelBuilder.Entity<UnitEntity>(e =>
        {
            e.ToTable("Unit");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
            e.Property(x => x.Abbreviation).HasColumnType("varchar(5)");
            e.HasOne(x => x.UnitType)
             .WithMany(x => x.Units)
             .HasForeignKey(x => x.UnitTypeId)
             .HasConstraintName("FK_Unit_ToUnitType");
            e.HasData(
                new UnitEntity { Id = 1, Name = "Millilitres", Abbreviation = "ml", UnitTypeId = 1 },
                new UnitEntity { Id = 2, Name = "Litres", Abbreviation = "l", UnitTypeId = 1 },
                new UnitEntity { Id = 3, Name = "Quantity", Abbreviation = null, UnitTypeId = 2 },
                new UnitEntity { Id = 4, Name = "Grams", Abbreviation = "g", UnitTypeId = 3 },
                new UnitEntity { Id = 5, Name = "Kilograms", Abbreviation = "kg", UnitTypeId = 3 });
        });

        modelBuilder.Entity<IngredientUnitEntity>(e =>
        {
            e.ToTable("IngredientUnit");
            e.HasKey(x => new { x.IngredientId, x.UnitId });
            e.HasOne(x => x.Ingredient)
             .WithMany(x => x.IngredientUnits)
             .HasForeignKey(x => x.IngredientId)
             .HasConstraintName("FK_IngredientUnit_ToIngredient");
            e.HasOne(x => x.Unit)
             .WithMany(x => x.IngredientUnits)
             .HasForeignKey(x => x.UnitId)
             .HasConstraintName("FK_IngredientUnit_ToUnit");
        });

        modelBuilder.Entity<RecipeIngredientEntity>(e =>
        {
            e.ToTable("RecipeIngredient");
            e.HasKey(x => new { x.RecipeId, x.IngredientId, x.UnitId });
            e.Property(x => x.Amount).HasColumnType("decimal(10,4)").IsRequired();
            e.HasOne(x => x.Recipe)
             .WithMany(x => x.RecipeIngredients)
             .HasForeignKey(x => x.RecipeId)
             .HasConstraintName("FK_RecipeIngredient_ToRecipe");
            e.HasOne(x => x.Ingredient)
             .WithMany()
             .HasForeignKey(x => x.IngredientId);
            e.HasOne(x => x.Unit)
             .WithMany()
             .HasForeignKey(x => x.UnitId);
        });
    }
}
