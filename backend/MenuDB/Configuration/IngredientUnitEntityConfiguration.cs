using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class IngredientUnitEntityConfiguration : IEntityTypeConfiguration<IngredientUnitEntity>
{
    public void Configure(EntityTypeBuilder<IngredientUnitEntity> builder)
    {
        builder.ToTable("IngredientUnit");
        builder.HasKey(x => new { x.IngredientId, x.UnitId });
        builder.HasOne(x => x.Ingredient)
            .WithMany(x => x.IngredientUnits)
            .HasForeignKey(x => x.IngredientId)
            .HasConstraintName("FK_IngredientUnit_ToIngredient");
        builder.HasOne(x => x.Unit)
            .WithMany(x => x.IngredientUnits)
            .HasForeignKey(x => x.UnitId)
            .HasConstraintName("FK_IngredientUnit_ToUnit");
    }
}
