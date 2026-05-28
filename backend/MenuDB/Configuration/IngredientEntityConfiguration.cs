using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class IngredientEntityConfiguration : IEntityTypeConfiguration<IngredientEntity>
{
    public void Configure(EntityTypeBuilder<IngredientEntity> builder)
    {
        builder.ToTable("Ingredient");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Ingredient_Name");
    }
}
