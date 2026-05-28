using MenuDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuDB.Configuration;

public class UnitTypeEntityConfiguration : IEntityTypeConfiguration<UnitTypeEntity>
{
    public void Configure(EntityTypeBuilder<UnitTypeEntity> builder)
    {
        builder.ToTable("UnitType");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnType("varchar(50)").IsRequired();
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("UX_UnitType_Name");
        builder.HasData(
            new UnitTypeEntity { Id = 1, Name = "Volume" },
            new UnitTypeEntity { Id = 2, Name = "Quantity" },
            new UnitTypeEntity { Id = 3, Name = "Weight" });
    }
}
