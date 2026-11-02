using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeSearchIndexEntityConfiguration : IEntityTypeConfiguration<RecipeSearchIndexEntity>
{
    public void Configure(EntityTypeBuilder<RecipeSearchIndexEntity> builder)
    {
        builder.ToTable("RecipeSearchIndex");
        builder.HasKey(x => x.RecipeId).HasName("PK_RecipeSearchIndex");
        builder.Property(x => x.RecipeId).ValueGeneratedNever();
        builder.Property(x => x.Title).HasColumnType("nvarchar(200)").IsRequired();
        builder.Property(x => x.SummaryText).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.IngredientsText).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.StepsText).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.AccessScope).HasColumnType("nvarchar(30)").IsRequired();
        builder.Property(x => x.IsPublishedExternally).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.HasOne(x => x.Recipe)
            .WithOne()
            .HasForeignKey<RecipeSearchIndexEntity>(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RecipeSearchIndex_ToRecipe");
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RecipeSearchIndex_ToMenuUser");
    }
}
