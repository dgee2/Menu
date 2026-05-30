using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeIngredientEntityConfiguration : IEntityTypeConfiguration<RecipeIngredientEntity>
{
    public void Configure(EntityTypeBuilder<RecipeIngredientEntity> builder)
    {
        builder.ToTable("RecipeIngredient");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.IngredientText).HasColumnType("nvarchar(200)").IsRequired();
        builder.Property(x => x.MeasureText).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.SectionTitle).HasColumnType("nvarchar(200)");
        builder.Property(x => x.Amount).HasColumnType("decimal(10,4)");
        builder.Property(x => x.UnitText).HasColumnType("nvarchar(50)");
        builder.Property(x => x.PreparationText).HasColumnType("nvarchar(200)");
        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeIngredients)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RecipeIngredient_ToRecipe");
        builder.HasOne(x => x.CanonicalIngredient)
            .WithMany()
            .HasForeignKey(x => x.CanonicalIngredientId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.CanonicalUnit)
            .WithMany()
            .HasForeignKey(x => x.CanonicalUnitId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
