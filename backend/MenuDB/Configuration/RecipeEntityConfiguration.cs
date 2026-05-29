using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeEntityConfiguration : IEntityTypeConfiguration<RecipeEntity>
{
    public void Configure(EntityTypeBuilder<RecipeEntity> builder)
    {
        builder.ToTable("Recipe");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.Name).HasColumnType("varchar(500)").IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_Recipe_Name");
    }
}
