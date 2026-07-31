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
        builder.Property(x => x.Title).HasColumnType("nvarchar(200)").IsRequired();
        builder.Property(x => x.OwnerUserId).IsRequired(false);
        builder.Property(x => x.AccessScope).HasColumnType("nvarchar(30)").IsRequired().HasDefaultValue("Private");
        builder.Property(x => x.Summary).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.Servings).IsRequired(false);
        builder.Property(x => x.YieldText).HasColumnType("nvarchar(100)").IsRequired(false);
        builder.Property(x => x.PrepTimeMinutes).IsRequired(false);
        builder.Property(x => x.CookTimeMinutes).IsRequired(false);
        builder.Property(x => x.TotalTimeMinutes).IsRequired(false);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2").IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(x => new { x.OwnerUserId, x.Title })
            .IsUnique()
            .HasDatabaseName("UX_Recipe_OwnerUserId_Title")
            .HasFilter(null);
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .HasConstraintName("FK_Recipe_ToMenuUser")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
