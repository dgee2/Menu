using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class RecipeAccessScopeEntityConfiguration : IEntityTypeConfiguration<RecipeAccessScopeEntity>
{
    /// <summary>
    /// The seeded lookup rows. Exposed so the drift guard in MenuApi.Tests can compare them against
    /// <c>MenuApi.ValueObjects.RecipeAccessScope</c> - MenuDB cannot reference that enum itself.
    /// </summary>
    public static IReadOnlyDictionary<byte, string> SeedRows { get; } = new Dictionary<byte, string>
    {
        [1] = "Private",
        [2] = "AuthenticatedUsers",
    };

    public void Configure(EntityTypeBuilder<RecipeAccessScopeEntity> builder)
    {
        builder.ToTable("RecipeAccessScope");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("tinyint").ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_RecipeAccessScope_Name");
        builder.HasData(SeedRows.Select(row => new RecipeAccessScopeEntity { Id = row.Key, Name = row.Value }));
    }
}
