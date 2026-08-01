using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeStepEntityConfiguration : IEntityTypeConfiguration<RecipeStepEntity>
{
    public void Configure(EntityTypeBuilder<RecipeStepEntity> builder)
    {
        builder.ToTable("RecipeStep");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasColumnType("nvarchar(200)").IsRequired(false);
        builder.Property(x => x.InstructionText).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.DurationMinutes).IsRequired(false);
        builder.HasOne(x => x.Recipe)
            .WithMany(x => x.RecipeSteps)
            .HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_RecipeStep_ToRecipe");
    }
}
