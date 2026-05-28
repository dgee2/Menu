using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeIngredientEntityConfiguration : IEntityTypeConfiguration<RecipeIngredientEntity>
{
    public void Configure(EntityTypeBuilder<RecipeIngredientEntity> builder)
    {
        builder.ToTable("RecipeIngredient");
        builder.HasKey(x => new { x.RecipeId, x.IngredientId, x.UnitId });
        builder.Property(x => x.Amount).HasColumnType("decimal(10,4)").IsRequired();
        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeIngredients)
            .HasForeignKey(x => x.RecipeId)
            .HasConstraintName("FK_RecipeIngredient_ToRecipe");
        builder.HasOne(x => x.Ingredient)
            .WithMany()
            .HasForeignKey(x => x.IngredientId);
        builder.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId);
    }
}
